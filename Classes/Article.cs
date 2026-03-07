namespace WSRobotitus.Classes;

public record Article(
    string Title,
    string Link,
    string ImageLink,
    string Description,
    string Author,
    DateTime Date,
    string Content,
    string Category,
    int ReadingTimeMinutes
) : NewsItem(Title, Link, ImageLink, Description, Author, Date);
