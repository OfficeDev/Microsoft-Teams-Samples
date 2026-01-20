// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.AgentSamples.Bots;
using Microsoft.AgentSamples.Models;
using Microsoft.AgentSamples.Services;

namespace Microsoft.AgentSamples.Controllers;

/// <summary>
/// Controller for home page views.
/// </summary>
public class HomeController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly DeeplinkHelper _deeplinkHelper = new();
    private const string TeamsUrl = "https://teams.microsoft.com/l/entity/";

    private static List<DeepLinksModel> _deeplinks = new();
    private static List<DeepLinkChannelModel> _channelDeeplinks = new();

    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    [Route("DeepLink")]
    public ActionResult DeepLink()
    {
        if (_deeplinks.Count == 0)
        {
            var appId = _configuration["MicrosoftAppId"] ?? string.Empty;
            var tabEntityId = _configuration["TabEntityId"] ?? string.Empty;

            _deeplinks.Add(new DeepLinksModel
            {
                LinkUrl = _deeplinkHelper.GetDeepLinkToTabTask(TeamsUrl, appId, tabEntityId, "topic1"),
                Id = 1,
                LinkTitle = "Bots in Teams"
            });

            _deeplinks.Add(new DeepLinksModel
            {
                LinkUrl = _deeplinkHelper.GetDeepLinkToTabTask(TeamsUrl, appId, tabEntityId, "topic2"),
                Id = 2,
                LinkTitle = "Bot Framework SDK"
            });

            _deeplinks.Add(new DeepLinksModel
            {
                LinkUrl = _deeplinkHelper.GetDeepLinkToTabTask(TeamsUrl, appId, tabEntityId, "topic3"),
                Id = 3,
                LinkTitle = "Teams Apps"
            });
        }

        return View(_deeplinks);
    }

    [Route("DeepLinkChannel")]
    public ActionResult DeepLinkChannel()
    {
        if (_channelDeeplinks.Count == 0)
        {
            PopulateChannelDeeplinks();
        }

        ViewBag.AppId = _configuration["TeamsAppId"];
        return View(_channelDeeplinks);
    }

    [Route("ChannelView")]
    public ActionResult ChannelView()
    {
        if (_channelDeeplinks.Count == 0)
        {
            PopulateChannelDeeplinks();
        }

        return View(_channelDeeplinks);
    }

    [Route("Configure")]
    public ActionResult Configure()
    {
        return View();
    }

    [Route("TaskList")]
    public ActionResult TaskList()
    {
        if (_deeplinks.Count == 0)
        {
            var appId = _configuration["MicrosoftAppId"] ?? string.Empty;
            var tabEntityId = _configuration["TabEntityId"] ?? string.Empty;

            _deeplinks.Add(new DeepLinksModel
            {
                LinkUrl = _deeplinkHelper.GetDeepLinkToTabTask(TeamsUrl, appId, tabEntityId, "topic1"),
                Id = 1,
                LinkTitle = "Bots in Teams"
            });

            _deeplinks.Add(new DeepLinksModel
            {
                LinkUrl = _deeplinkHelper.GetDeepLinkToTabTask(TeamsUrl, appId, tabEntityId, "topic2"),
                Id = 2,
                LinkTitle = "Bot Framework SDK"
            });

            _deeplinks.Add(new DeepLinksModel
            {
                LinkUrl = _deeplinkHelper.GetDeepLinkToTabTask(TeamsUrl, appId, tabEntityId, "topic3"),
                Id = 3,
                LinkTitle = "Teams Apps"
            });
        }

        return View(_deeplinks);
    }

    [Route("Detail/{id}")]
    public ActionResult Detail(int id)
    {
        return View(_deeplinks.FirstOrDefault(i => i.Id == id));
    }

    [Route("DetailChannel/{id}")]
    public ActionResult DetailChannel(int id)
    {
        var model = new DeepLinkChannelModel
        {
            Id = id
        };

        return View(model);
    }

    [Route("ExtendedDeepLinks")]
    public PartialViewResult ExtendedDeepLinks()
    {
        return PartialView();
    }

    private void PopulateChannelDeeplinks()
    {
        var channelId = DeepLinkBot.ChannelId;
        var appId = _configuration["MicrosoftAppId"] ?? string.Empty;
        var baseUrl = _configuration["BaseURL"] ?? string.Empty;
        var channelEntityId = _configuration["ChannelEntityId"] ?? string.Empty;

        _channelDeeplinks.Add(new DeepLinkChannelModel
        {
            LinkUrl = _deeplinkHelper.GetDeepLinkToChannelTask(TeamsUrl, appId, baseUrl, channelId, channelEntityId, "bot1"),
            Id = 1,
            LinkTitle = "Bots in Teams"
        });

        _channelDeeplinks.Add(new DeepLinkChannelModel
        {
            LinkUrl = _deeplinkHelper.GetDeepLinkToChannelTask(TeamsUrl, appId, baseUrl, channelId, channelEntityId, "bot2"),
            Id = 2,
            LinkTitle = "Bot Framework SDK"
        });

        _channelDeeplinks.Add(new DeepLinkChannelModel
        {
            LinkUrl = _deeplinkHelper.GetDeepLinkToChannelTask(TeamsUrl, appId, baseUrl, channelId, channelEntityId, "bot3"),
            Id = 3,
            LinkTitle = "Teams Apps"
        });
    }
}
