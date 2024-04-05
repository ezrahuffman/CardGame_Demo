public interface IPlayer
{
    public HealthSystem HealthSystem { get; }
    public Hand Hand { get; }
    public HealthSystem.OnDie OnDieEvent { set; }
    public bool CanDraw { get; }
    public bool HasPlayed
    {get;}

    public bool canPlay { get; set; }
    public void Discard(Card card);
    public void HasPerformedAction(Card card);
    public void DealDmg(float amnt);
}
