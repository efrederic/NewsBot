using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using NewsAPI.Models;
using NewsAPI.Constants;

namespace TestBotApplication.Dialogs {
    [Serializable]
    public class RootDialog : IDialog<object> {

        public Task StartAsync(IDialogContext context) {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result) {
            var messageActivity = await result;

            PromptDialog.Text(context, RetrieveArticle, "Hi! I can find news articles about subjects you are interested in. What topic would you like to see news about? If you would like to view multiple articles, please include a number in parenthesis with your request.");
        }

        private async Task RetrieveArticle(IDialogContext context, IAwaitable<string> result) {

            var userInput = await result;

            var numberMatch = Regex.Match(userInput, @"\(\d+\)").Value;
            var numResults = string.IsNullOrEmpty(numberMatch) ? 1 : int.Parse(numberMatch.Substring(1, numberMatch.Length - 2));
            userInput = Regex.Replace(userInput, @"\(\d+\)", "");

            var articlesResult = await MessagesController.NewsApiClient.GetTopHeadlinesAsync(new TopHeadlinesRequest {
                Q = userInput,
                PageSize = numResults,
                Language = Languages.EN
            });

            if (articlesResult.Status == Statuses.Ok) {
                if (articlesResult.TotalResults == 0) {
                    await context.PostAsync($"No results found for query '{userInput}'");
                    context.Wait(MessageReceivedAsync);
                }
                await context.PostAsync($"{articlesResult.Articles.Count} results found for query '{userInput}'");
                foreach (var article in articlesResult.Articles) {
                    var message = context.MakeMessage();
                    var heroCard = new HeroCard {
                        Title = article.Title,
                        Subtitle = article.Description,
                        Text = "Powered by NewsAPI.org",
                        Images = new List<CardImage> {
                            new CardImage(article.UrlToImage)
                        },
                        Buttons = new List<CardAction> {
                            new CardAction(ActionTypes.OpenUrl, "Go to article", article.Url)
                        }
                    };
                    message.Attachments.Add(heroCard.ToAttachment());
                    await context.PostAsync(message);
                }
            }
            else {
                await context.PostAsync("Error when trying to find news articles.");
            }
            context.Wait(MessageReceivedAsync);
        }
    }
}