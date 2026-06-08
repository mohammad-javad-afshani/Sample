using Domain.Products;

namespace Domain.Reviews;

public class Review
{
    public ReviewId Id { get; private set; }
    public ProductId ProductId { get; private set; }
    public string Author { get; private set; } = string.Empty;
    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    private Review() { }

    public Review(ProductId productId, string author, int rating, string comment)
    {
        Id = new ReviewId(Guid.NewGuid());
        ProductId = productId;
        Author = author;
        Rating = rating;
        Comment = comment;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
