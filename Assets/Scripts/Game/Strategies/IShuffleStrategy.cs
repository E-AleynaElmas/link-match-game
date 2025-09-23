
using LinkMatch.Game.Board;

namespace LinkMatch.Game.Strategies
{
    public interface IShuffleStrategy
    {
        /// Tahtada 4-yön bağlantı ile aynı renkten 3+’luk herhangi bir komponent var mı?
        bool HasAnyMove(BoardModel model);

        /// Tahtayı karıştır.
        void Shuffle(BoardModel model, System.Random rng);
    }
}