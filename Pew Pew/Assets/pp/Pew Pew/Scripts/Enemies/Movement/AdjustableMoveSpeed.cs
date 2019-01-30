namespace GameCore
{
    /// <summary>
    /// Contract for any object that has an adjustable move speed.
    /// </summary>
    public interface AdjustableMoveSpeed
    {
        void IncrementSpeed();
    }

    /// <summary>
    /// Contract for any object that has an adjustable shoot speed.
    /// </summary>
    public interface AdjustableShootSpeed
    {
        void IncrementSpeed();
    }
}