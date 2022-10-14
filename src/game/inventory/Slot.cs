using MinicraftGame.Game.ItemType;

namespace MinicraftGame.Game.Inventories
{
    public class Slot
    {
        private const int SLOT_MAX = 255;

        public Item Item { get; private set; }
        public int Amount { get; private set; }

        public bool IsEmpty => Item == Items.Nothing || Amount <= 0;
        public bool IsFull => Amount >= SLOT_MAX;

        public Slot() => Set(Items.Nothing, 0);

        // adds amount to slot without going over max and returns remainder
        public int? Add(int amount)
        {
            // find remainder of slot capacity
            var remainingCapacity = SLOT_MAX - Amount;
            // if no capacity, do nothing and return amount given
            if (remainingCapacity == 0)
                return amount;
            // if capacity allows for amount, add to slot and return none left
            if (remainingCapacity >= amount)
            {
                Amount += amount;
                return null;
            }
            // capacity is less than amount, add remaining capacity and return amoutn left;
            Amount = SLOT_MAX;
            return amount - remainingCapacity;
        }

        public int? Set(Item item, int amount)
        {
            Item = item;
            Amount = 0;
            return Add(amount);
        }

        public void Decrement() => Amount--;
    }
}
