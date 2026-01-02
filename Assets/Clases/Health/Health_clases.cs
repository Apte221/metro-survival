public class Health
{
    public int Max { get; }
    public int Curent { get; private set; }

    public Health(int max)
    {
        Max = max;
        Curent = max;
    }

    public bool TakeDamage(int damage)
    {
        Curent -= damage;
        if (Curent < 0)
        {
            Curent = 0;
        }
        return Curent == 0;
    }

    public void heal(int amount)
    {
        Curent += amount;
        if (Curent > Max)
        {
            Curent = Max;
        }
    }





}