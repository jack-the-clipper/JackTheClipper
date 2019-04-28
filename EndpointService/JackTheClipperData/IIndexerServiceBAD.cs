using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using JackTheClipperCommon.SharedClasses;

namespace JackTheClipperData
{
    //TODO: TIM - entspricht nicht dem Dokukonzept.

    public interface IIndexerService
    {
        MethodResult IndexRssFeedItemThreadSafe(SyndicationItem syndication, Guid feedId);
        void ClearIndex();
        IReadOnlyCollection<ShortArticle> GetFeed(User user, Feed feed);
        Article GetArticle(User user, Guid articleId);
    }
}
