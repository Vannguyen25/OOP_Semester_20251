namespace OOP_Semester.Models
{
    // Dùng cho User.Role
    public enum UserRole
    {
        Admin,
        User
    }

    // Dùng cho Habit.GoalUnitType và HabitTemplate
    public enum GoalUnitType
    {
        Count,  // Đếm số lần
        Time,   // Thời gian
        Boolean // Xong/Chưa xong
    }

    // Dùng cho Habit.Status
    public enum HabitStatus
    {
        Active,
        Completed,
        Dropped
    }

    // Dùng cho HabitLogs.TimeOfDay
    public enum TimeOfDay
    {
        Morning,
        Afternoon,
        Evening,
        Anytime
    }
}