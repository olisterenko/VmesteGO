using VmesteGO.Domain.Entities;
using VmesteGO.Domain.Enums;
using VmesteGO.Dto.Responses;

namespace VmesteGO.Extensions;

public static class EventEntityExt
{
    public static EventResponse ToEventResponse(
        this Event source,
        Func<string, string> getImageUrlByKey,
        EventStatus? eventStatus = null)
    {
        return new EventResponse
        {
            Id = source.Id,
            Title = source.Title,
            Dates = source.Dates,
            Location = source.Location,
            Description = source.Description,
            AgeRestriction = source.AgeRestriction,
            Price = source.Price,
            IsPrivate = source.IsPrivate,
            ExternalId = source.ExternalId,
            CreatorId = source.CreatorId,
            CreatorUsername = source.Creator?.Username ?? "Unknown",
            Categories = source.EventCategories.Select(ec => ec.Category.Name).ToList(),
            Images = source.EventImages.Select(img => getImageUrlByKey(img.ImageKey)).ToList(),
            EventStatus = eventStatus
        };
    }
}