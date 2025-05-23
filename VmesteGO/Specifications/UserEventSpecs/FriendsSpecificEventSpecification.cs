﻿using Ardalis.Specification;
using VmesteGO.Domain.Entities;

namespace VmesteGO.Specifications.UserEventSpecs;

public sealed class FriendsSpecificEventSpecification : Specification<UserEvent>
{
    public FriendsSpecificEventSpecification(int eventId, IEnumerable<int> friendIds)
    {
        Query.Where(ea => friendIds.Contains(ea.UserId))
            .Where(ea => ea.Event.Id == eventId)
            .Include(ea => ea.Event)
            .Include(ea => ea.User);
    }
}