public record c_Article(
    string Title,
    string Link,
    string ImageLink,
    string Description,
    string Author,
    DateTime Date,
    string Content,
    string Category,
    int ReadingTimeMinutes
) : c_NewsItem(Title, Link, ImageLink, Description, Author, Date);
