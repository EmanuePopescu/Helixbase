﻿using Helixbase.Feature.Redirects.Models;
using Helixbase.Foundation.Content.Repositories;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.Pipelines.HttpRequest;
using System;
using System.Web;

namespace Helixbase.Feature.Redirects.Pipelines
{
    public class RedirectResolver : HttpRequestProcessor
    {
        private IContentRepository _contentRepository;

        public RedirectResolver(IContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
        }

        public override void Process(HttpRequestArgs args)
        {
            if (args.Context.Request.Url.OriginalString.ToLower().Contains("/sitecore") || args.Context.Request.Url.AbsolutePath.Equals("/"))
                return;
            // only perform redirect if no Sitecore item exists
            if (Sitecore.Context.Item == null)
                Perform301Redirect();
        }

        private void Perform301Redirect()
        {
            var redirectFolder = _contentRepository.QuerySingle<IRedirectFolder>($"fast:{Sitecore.Context.Site.RootPath}/*[@@templateid='{Helixbase.Foundation.Content.Templates.GlobalFolder.TemplateId.ToString("B").ToUpper()}']/*[@@templateid='{Templates.RedirectFolder.TemplateId.ToString("B").ToUpper()}']");

            var path = HttpContext.Current.Request.Url.LocalPath;

            if (redirectFolder == null)
                throw new NullReferenceException("Redirect folder not found");

            foreach (var redirect in redirectFolder.Children)
            {
                // TODO - make Infer Types work with Helix
                //    if (redirect is I301Redirect)
                //    {
                //        var redirect301Item = redirect as I301Redirect;
                //        if (redirect301Item.RequestedURL?.ToLower() == path.ToLower())
                //        {
                //            var targetItem = _contentRepository.GetContentItem<Item>(redirect301Item.RedirectItem.Id.ToString());
                //            HttpContext.Current.Response.RedirectPermanent(LinkManager.GetItemUrl(targetItem), true);
                //        }
                //    }
                if (string.IsNullOrEmpty(redirect.RequestedURL))
                    throw new NullReferenceException("Could not find a URL value on the redirect item");

                if (redirect.RequestedURL.ToLower() == path.ToLower())
                {
                    var targetItem = _contentRepository.GetContentItem<Item>(redirect.RedirectItem.Id.ToString());
                    HttpContext.Current.Response.RedirectPermanent(LinkManager.GetItemUrl(targetItem), true);
                }
            }
        }
    }
}