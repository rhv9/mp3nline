namespace youtube_dl_api.DB
{
    public record Burger
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class BurgerDB
    {
        private static List<Burger> _burgers = new List<Burger>()
        { 
            new Burger{ Id=1, Name="McDonalds McChicken Burger"},
            new Burger{ Id=2, Name="McDonalds Mayo Chicken Burger"},
            new Burger{ Id=3, Name="McDonalds BigMac Burger"}
        };

        public static List<Burger> GetBurgers()
        {
            return _burgers; 
        }

        public static Burger ? GetBurger(int id)
        {
            return _burgers.SingleOrDefault(b => b.Id == id);
        }

        public static Burger CreateBurger(Burger burger)
        {
            _burgers.Add(burger);
            return burger;
        }

        public static Burger UpdateBurger(Burger update)
        {
            _burgers = _burgers.Select(burger =>
            {
                if (burger.Id == update.Id)
                {
                    burger.Name = update.Name;
                }
                return burger;
            }).ToList();
            return update;
        }

        public static void RemoveBurger(int id)
        {
            _burgers = _burgers.FindAll(burger => burger.Id != id).ToList();
        }
    }
}
