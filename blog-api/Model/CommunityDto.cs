﻿namespace blog_api.Model;

public class CommunityDto
{
    public required Guid Id { get; set; }
    
    public required DateTime CreationTime { get; set; }
    
    public required string Name { get; set; }
    
    public required string Description { get; set; }
    
    public required bool IsClosed { get; set; }
    
    public required int SubscribersCount { get; set; }
}