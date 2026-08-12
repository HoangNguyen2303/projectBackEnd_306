namespace EduTrack.Application.Common;

/// <summary>Ném khi không tìm thấy entity theo id — Api layer map thành 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Ném khi user không có quyền trên đúng bản ghi này — Api layer map thành 403.</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
