namespace WSRobotitus.Classes;

public record NewsItem(
    string Title,
    string Link,
    string ImageLink,
    string Description,
    string Author,
    DateTime Date
);
