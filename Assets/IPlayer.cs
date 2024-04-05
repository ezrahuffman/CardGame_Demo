public interface IPlayer
{
    public HealthSystem HealthSystem { get; }
    public Hand Hand { get; }
    public HealthSystem.OnDie OnDieEvent { set; }
    public bool CanDraw { get; }
    public bool HasPlayed
    {get;}

    public bool canPlay { get; set; }
}
