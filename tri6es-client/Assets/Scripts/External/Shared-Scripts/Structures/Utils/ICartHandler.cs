using System.Collections.Generic;

namespace Shared.Structures
{
    public interface ICartHandler
    {
        List<Cart> Carts { get; set; }

        void HandleCarts();

        void AddCart(Cart cart);
    }
}
