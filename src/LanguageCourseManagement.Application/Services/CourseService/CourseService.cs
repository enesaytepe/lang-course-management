using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using LanguageCourseManagement.Application.Common.Requests;
using LanguageCourseManagement.Application.Common.Responses;
using LanguageCourseManagement.Application.DTOs.Courses;
using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.Domain.DTOs;
using LanguageCourseManagement.Domain.Entities;
using LanguageCourseManagement.Domain.Enums;
using LanguageCourseManagement.Domain.Paging;
using LanguageCourseManagement.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace LanguageCourseManagement.Application.Services.CourseService;

public sealed class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IOfferedLanguageRepository _offeredLanguageRepository;
    private readonly ICourseLevelRepository _courseLevelRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IClassroomRepository _classroomRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CourseService> _logger;
    private readonly IValidator<CreateCourseRequest> _createValidator;
    private readonly IValidator<UpdateCourseRequest> _updateValidator;

    public CourseService(
        ICourseRepository courseRepository,
        IBranchRepository branchRepository,
        IOfferedLanguageRepository offeredLanguageRepository,
        ICourseLevelRepository courseLevelRepository,
        ITeacherRepository teacherRepository,
        IClassroomRepository classroomRepository,
        IEnrollmentRepository enrollmentRepository,
        IMapper mapper,
        ILogger<CourseService> logger,
        IValidator<CreateCourseRequest> createValidator,
        IValidator<UpdateCourseRequest> updateValidator)
    {
        _courseRepository = courseRepository;
        _branchRepository = branchRepository;
        _offeredLanguageRepository = offeredLanguageRepository;
        _courseLevelRepository = courseLevelRepository;
        _teacherRepository = teacherRepository;
        _classroomRepository = classroomRepository;
        _enrollmentRepository = enrollmentRepository;
        _mapper = mapper;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<CourseResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository.Query()
            .Where(item => item.Id == id)
            .ProjectTo<CourseResponse>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
            throw new NotFoundException("Ders bulunamadı.");

        return course;
    }

    public async Task<GetListResponse<CourseListResponse>> GetListAsync(
        PageRequest pageRequest,
        string? search,
        Guid? branchId,
        Guid? offeredLanguageId,
        bool? isActive,
        CourseStatus? status = null,
        bool showDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        Expression<Func<Course, bool>> predicate = course =>
            (!branchId.HasValue || course.BranchId == branchId.Value) &&
            (!offeredLanguageId.HasValue || course.OfferedLanguageId == offeredLanguageId.Value) &&
            (!isActive.HasValue || course.IsActive == isActive.Value) &&
            (!status.HasValue || course.Status == status.Value) &&
            (normalizedSearch == null ||
             course.Name.Contains(normalizedSearch) ||
             (course.Branch != null && course.Branch.Name.Contains(normalizedSearch)) ||
             (course.OfferedLanguage != null && course.OfferedLanguage.Name.Contains(normalizedSearch)) ||
             (course.CourseLevel != null && course.CourseLevel.Name.Contains(normalizedSearch)) ||
             (course.Teacher != null && (course.Teacher.FirstName.Contains(normalizedSearch) || course.Teacher.LastName.Contains(normalizedSearch))) ||
             (course.Classroom != null && course.Classroom.Name.Contains(normalizedSearch)));

        var courseQuery = showDeleted ? _courseRepository.QueryWithIgnoreFilters() : _courseRepository.Query();
        var courses = await courseQuery
            .Where(predicate)
            .OrderByDescending(course => course.StartDate).ThenBy(course => course.Name)
            .ProjectTo<CourseListResponse>(_mapper.ConfigurationProvider)
            .ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize, cancellationToken: cancellationToken);

        return new GetListResponse<CourseListResponse>
        {
            Index = courses.Index,
            Size = courses.Size,
            Count = courses.Count,
            Pages = courses.Pages,
            HasPrevious = courses.HasPrevious,
            HasNext = courses.HasNext,
            Items = courses.Items.ToList()
        };
    }

    public async Task<CourseResponse> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_createValidator, request, cancellationToken);

        request.Name = request.Name.Trim();

        await EnsureCourseNameUniqueAsync(request.Name, request.CourseLevelId, excludeCourseId: null, cancellationToken);
        await EnsureBranchExistsAsync(request.BranchId, requireActive: true, cancellationToken);
        await EnsureLanguageExistsAsync(request.OfferedLanguageId, requireActive: true, cancellationToken);
        await EnsureLevelExistsAsync(request.CourseLevelId, request.OfferedLanguageId, requireActive: true, cancellationToken);
        await EnsureTeacherExistsAsync(request.TeacherId, request.OfferedLanguageId, request.BranchId, requireActive: true, cancellationToken);
        var classroom = await EnsureClassroomExistsAsync(request.ClassroomId, request.BranchId, requireActive: true, cancellationToken);

        if (request.Capacity > classroom.Capacity)
            throw new BusinessException("Ders kapasitesi derslik kapasitesinden büyük olamaz.");

        var course = _mapper.Map<Course>(request);
        course.Id = Guid.NewGuid();
        course.IsActive = true;
        await ValidateScheduleRulesAsync(request.Schedules, request.StartDate, request.EndDate, request.TeacherId, request.ClassroomId, null, cancellationToken);
        course.Schedules = request.Schedules.Select(schedule => ToEntity(schedule, course.Id)).ToList();

        await _courseRepository.AddAsync(course, cancellationToken);

        _logger.LogInformation("[CourseService] Yeni ders oluşturuldu - {CourseId}", course.Id);

        return await GetByIdAsync(course.Id, cancellationToken);
    }

    public async Task<CourseResponse> UpdateAsync(Guid id, UpdateCourseRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_updateValidator, request, cancellationToken);

        request.Name = request.Name.Trim();

        await EnsureCourseNameUniqueAsync(request.Name, request.CourseLevelId, excludeCourseId: id, cancellationToken);

        var course = await _courseRepository.GetAsync(
            item => item.Id == id,
            include: query => query.Include(item => item.Schedules!),
            cancellationToken: cancellationToken);
        if (course is null)
            throw new NotFoundException("Ders bulunamadı.");

        await EnsureBranchExistsAsync(request.BranchId, request.BranchId != course.BranchId, cancellationToken);
        await EnsureLanguageExistsAsync(request.OfferedLanguageId, request.OfferedLanguageId != course.OfferedLanguageId, cancellationToken);
        await EnsureLevelExistsAsync(request.CourseLevelId, request.OfferedLanguageId, request.CourseLevelId != course.CourseLevelId || request.OfferedLanguageId != course.OfferedLanguageId, cancellationToken);
        await EnsureTeacherExistsAsync(request.TeacherId, request.OfferedLanguageId, request.BranchId, request.TeacherId != course.TeacherId, cancellationToken);
        var classroom = await EnsureClassroomExistsAsync(request.ClassroomId, request.BranchId, request.ClassroomId != course.ClassroomId || request.BranchId != course.BranchId, cancellationToken);
        await ValidateScheduleRulesAsync(request.Schedules, request.StartDate, request.EndDate, request.TeacherId, request.ClassroomId, id, cancellationToken);

        if (request.Capacity > classroom.Capacity)
            throw new BusinessException("Ders kapasitesi derslik kapasitesinden büyük olamaz.");

        if (request.Status is CourseStatus.Open)
            await EnsureAllDependenciesActiveAsync(request, cancellationToken);

        course.Name = request.Name;
        course.BranchId = request.BranchId;
        course.OfferedLanguageId = request.OfferedLanguageId;
        course.CourseLevelId = request.CourseLevelId;
        course.TeacherId = request.TeacherId;
        course.ClassroomId = request.ClassroomId;
        course.StartDate = request.StartDate;
        course.EndDate = request.EndDate;
        course.Capacity = request.Capacity;
        course.TuitionFee = request.TuitionFee;
        course.Status = request.Status;
        course.IsActive = request.IsActive;
        course.Schedules ??= [];
        course.Schedules.Clear();
        foreach (var schedule in request.Schedules)
            course.Schedules.Add(ToEntity(schedule, course.Id));

        await _courseRepository.UpdateAsync(course, cancellationToken);

        _logger.LogInformation("[CourseService] Ders güncellendi - {CourseId}", course.Id);

        return await GetByIdAsync(course.Id, cancellationToken);
    }

    public async Task<CourseResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await _courseRepository.GetAsync(
            item => item.Id == id,
            cancellationToken: cancellationToken);

        if (course is null)
            throw new NotFoundException("Ders bulunamadı.");

        var hasSchedule = await _courseRepository.AnyAsync(
            c => c.Id == id && c.Schedules != null && c.Schedules.Any(),
            cancellationToken: cancellationToken);

        var hasActiveEnrollment = await _enrollmentRepository.AnyAsync(
            e => e.CourseId == id && e.Status == EnrollmentStatus.Active,
            cancellationToken: cancellationToken);

        if (hasSchedule || hasActiveEnrollment)
            throw new BusinessException("Derse ait ders programı veya aktif öğrenci kaydı bulunduğundan ders silinemez.");

        var response = _mapper.Map<CourseResponse>(course);
        await _courseRepository.DeleteAsync(course, cancellationToken);

        _logger.LogInformation("[CourseService] Ders silindi - {CourseId}", course.Id);

        return response;
    }

    public async Task<IReadOnlyList<CourseScheduleItemDto>> GetSchedulesAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        var courseExists = await _courseRepository.AnyAsync(
            item => item.Id == courseId,
            cancellationToken: cancellationToken);

        if (!courseExists)
            throw new NotFoundException("Ders bulunamadı.");

        var schedules = await _courseRepository.Query()
            .Where(c => c.Id == courseId)
            .SelectMany(c => c.Schedules ?? Enumerable.Empty<CourseSchedule>())
            .Select(schedule => new CourseScheduleItemDto
            {
                Id = schedule.Id,
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime
            })
            .ToListAsync(cancellationToken);

        return schedules;
    }

    public async Task<IReadOnlyList<EligibleTeacherResponse>> GetEligibleTeachersAsync(
        Guid branchId, Guid offeredLanguageId, Guid courseLevelId,
        DateOnly startDate, DateOnly endDate,
        IReadOnlyList<CourseScheduleItemDto> schedules,
        Guid? excludeCourseId = null, CancellationToken cancellationToken = default)
    {
        var slots = schedules.Select(s => new ScheduleSlot(s.DayOfWeek, s.StartTime, s.EndTime)).ToList();
        var teachers = await _teacherRepository.GetEligibleTeachersAsync(
            branchId, offeredLanguageId, slots, startDate, endDate, excludeCourseId, cancellationToken);

        return teachers
            .Select(t => new EligibleTeacherResponse { Id = t.Id, FirstName = t.FirstName, LastName = t.LastName })
            .ToList();
    }

    public async Task<IReadOnlyList<EligibleClassroomResponse>> GetEligibleClassroomsAsync(
        Guid branchId, DateOnly startDate, DateOnly endDate,
        IReadOnlyList<CourseScheduleItemDto> schedules,
        Guid? excludeCourseId = null, CancellationToken cancellationToken = default)
    {
        var slots = schedules.Select(s => new ScheduleSlot(s.DayOfWeek, s.StartTime, s.EndTime)).ToList();
        var classrooms = await _classroomRepository.GetEligibleClassroomsAsync(
            branchId, slots, startDate, endDate, excludeCourseId, cancellationToken);

        return classrooms
            .Select(c => new EligibleClassroomResponse { Id = c.Id, Name = c.Name, Capacity = c.Capacity })
            .ToList();
    }

    private async Task ValidateScheduleRulesAsync(
        IReadOnlyList<CourseScheduleItemDto> schedules,
        DateOnly startDate,
        DateOnly endDate,
        Guid teacherId,
        Guid classroomId,
        Guid? excludeCourseId,
        CancellationToken cancellationToken)
    {
        if (schedules.Count == 0)
            throw new BusinessException("En az bir ders günü ve saati eklenmelidir.");
        if (schedules.Any(schedule => schedule.StartTime >= schedule.EndTime))
            throw new BusinessException("Ders başlangıç saati bitiş saatinden önce olmalıdır.");
        if (schedules.GroupBy(schedule => schedule.DayOfWeek).Any(group => group.Count() > 1))
            throw new BusinessException("Aynı gün için birden fazla ders programı eklenemez.");

        var teacher = await _teacherRepository.GetAsync(
            item => item.Id == teacherId,
            include: query => query.Include(item => item.TeacherLanguages)
                .Include(item => item.TeacherBranches)
                .Include(item => item.Availabilities)
                .Include(item => item.Courses!).ThenInclude(course => course.Schedules!),
            enableTracking: false,
            cancellationToken: cancellationToken);
        if (teacher is null || !teacher.IsActive)
            throw new BusinessException("Seçilen öğretmen uygun değil.");
        if (!schedules.All(schedule => (teacher.Availabilities ?? []).Any(availability =>
                availability.DayOfWeek == schedule.DayOfWeek &&
                availability.StartTime <= schedule.StartTime && availability.EndTime >= schedule.EndTime)))
            throw new BusinessException("Öğretmen seçilen ders saatlerinin tamamında müsait değil.");
        if (HasCourseConflict(teacher.Courses ?? [], startDate, endDate, schedules, excludeCourseId))
            throw new BusinessException("Öğretmenin seçilen tarihlerde çakışan bir dersi bulunuyor.");

        var classroom = await _classroomRepository.GetAsync(
            item => item.Id == classroomId,
            include: query => query.Include(item => item.Courses!).ThenInclude(course => course.Schedules!),
            enableTracking: false,
            cancellationToken: cancellationToken);
        if (classroom is null || !classroom.IsActive)
            throw new BusinessException("Seçilen derslik uygun değil.");
        if (HasCourseConflict(classroom.Courses ?? [], startDate, endDate, schedules, excludeCourseId))
            throw new BusinessException("Dersliğin seçilen tarihlerde çakışan bir dersi bulunuyor.");
    }


    private static bool HasCourseConflict(IEnumerable<Course> courses, DateOnly startDate, DateOnly endDate,
        IReadOnlyList<CourseScheduleItemDto> schedules, Guid? excludeCourseId)
    {
        return courses.Where(course => course.Status == CourseStatus.Open)
                .Where(course => !excludeCourseId.HasValue || course.Id != excludeCourseId.Value)
                .Where(course => course.StartDate <= endDate && course.EndDate >= startDate)
                .SelectMany(course => course.Schedules ?? [])
                .Any(existing => schedules.Any(schedule => existing.DayOfWeek == schedule.DayOfWeek &&
                    existing.StartTime < schedule.EndTime && existing.EndTime > schedule.StartTime));
    }

    private static CourseSchedule ToEntity(CourseScheduleItemDto schedule, Guid? courseId = null)
    {
        return new()
        {
            Id = schedule.Id ?? Guid.NewGuid(),
            CourseId = courseId ?? Guid.Empty,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime
        };
    }

    private async Task EnsureBranchExistsAsync(Guid branchId, bool requireActive, CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.GetAsync(
            item => item.Id == branchId,
            enableTracking: false,
            cancellationToken: cancellationToken);

        if (branch is null)
            throw new BusinessException("Seçilen şube bulunamadı.");

        if (requireActive && !branch.IsActive)
            throw new BusinessException("Ders yalnızca aktif bir şubede açılabilir.");
    }

    private async Task EnsureLanguageExistsAsync(Guid languageId, bool requireActive, CancellationToken cancellationToken)
    {
        var language = await _offeredLanguageRepository.GetAsync(
            item => item.Id == languageId,
            enableTracking: false,
            cancellationToken: cancellationToken);

        if (language is null)
            throw new BusinessException("Seçilen dil bulunamadı.");

        if (requireActive && !language.IsActive)
            throw new BusinessException("Ders yalnızca aktif bir dil için açılabilir.");
    }

    private async Task EnsureLevelExistsAsync(Guid courseLevelId, Guid offeredLanguageId, bool requireActive, CancellationToken cancellationToken)
    {
        var level = await _courseLevelRepository.GetAsync(
            item => item.Id == courseLevelId,
            enableTracking: false,
            cancellationToken: cancellationToken);

        if (level is null)
            throw new BusinessException("Seçilen kurs seviyesi bulunamadı.");

        if (level.OfferedLanguageId != offeredLanguageId)
            throw new BusinessException("Seçilen kurs seviyesi seçilen dile ait değil.");

        if (requireActive && !level.IsActive)
            throw new BusinessException("Ders yalnızca aktif bir kurs seviyesi ile açılabilir.");
    }

    private async Task EnsureTeacherExistsAsync(Guid teacherId, Guid offeredLanguageId, Guid branchId, bool requireActive, CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.GetAsync(
            item => item.Id == teacherId,
            include: query => query
                .Include(item => item.TeacherLanguages)
                .Include(item => item.TeacherBranches!),
            enableTracking: false,
            cancellationToken: cancellationToken);

        if (teacher is null)
            throw new BusinessException("Seçilen öğretmen bulunamadı.");

        if (requireActive && !teacher.IsActive)
            throw new BusinessException("Derse yalnızca aktif bir öğretmen atanabilir.");

        if (teacher.TeacherLanguages is null || !teacher.TeacherLanguages.Any(item => item.OfferedLanguageId == offeredLanguageId))
            throw new BusinessException("Seçilen öğretmen bu dili öğretemiyor.");

        if (teacher.TeacherBranches is null || !teacher.TeacherBranches.Any(item => item.BranchId == branchId))
            throw new BusinessException("Seçilen öğretmen bu şubede ders veremiyor.");
    }

    private async Task<Classroom> EnsureClassroomExistsAsync(Guid classroomId, Guid branchId, bool requireActive, CancellationToken cancellationToken)
    {
        var classroom = await _classroomRepository.GetAsync(
            item => item.Id == classroomId,
            enableTracking: false,
            cancellationToken: cancellationToken);

        if (classroom is null)
            throw new BusinessException("Seçilen derslik bulunamadı.");

        if (classroom.BranchId != branchId)
            throw new BusinessException("Seçilen derslik seçilen şubeye ait değil.");

        if (requireActive && !classroom.IsActive)
            throw new BusinessException("Derse yalnızca aktif bir derslik atanabilir.");

        return classroom;
    }

    private async Task EnsureAllDependenciesActiveAsync(UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        await EnsureBranchExistsAsync(request.BranchId, requireActive: true, cancellationToken);
        await EnsureLanguageExistsAsync(request.OfferedLanguageId, requireActive: true, cancellationToken);
        await EnsureLevelExistsAsync(request.CourseLevelId, request.OfferedLanguageId, requireActive: true, cancellationToken);
        await EnsureTeacherExistsAsync(request.TeacherId, request.OfferedLanguageId, request.BranchId, requireActive: true, cancellationToken);
        await EnsureClassroomExistsAsync(request.ClassroomId, request.BranchId, requireActive: true, cancellationToken);
    }

    private async Task EnsureCourseNameUniqueAsync(string name, Guid courseLevelId, Guid? excludeCourseId, CancellationToken cancellationToken)
    {
        var nameExists = await _courseRepository.AnyAsync(
            c => c.Name == name && c.CourseLevelId == courseLevelId && (!excludeCourseId.HasValue || c.Id != excludeCourseId.Value),
            cancellationToken: cancellationToken);

        if (nameExists)
            throw new BusinessException("Bu seviyede aynı isimde kurs zaten mevcut.");
    }

    private static async Task ValidateAsync<TRequest>(IValidator<TRequest> validator, TRequest request, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (result.IsValid)
            return;

        throw new LanguageCourseManagement.Application.Exceptions.ValidationException(result.Errors
            .GroupBy(error => error.PropertyName)
            .Select(group => new LanguageCourseManagement.Application.Exceptions.ValidationExceptionModel
            {
                Property = group.Key,
                Errors = group.Select(error => error.ErrorMessage).ToArray()
            })
            .ToArray());
    }
}
