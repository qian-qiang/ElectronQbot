using System.Net.Http.Json;
using System.Text.Json;
using Contracts.Services;
using ElectronBot.Braincase;
using ElectronBot.Braincase.Contracts.Services;
using ElectronBot.Braincase.Helpers;
using Models;
using Sdcb.SparkDesk;

namespace Services;
public class SparkDeskChatbotClient : IChatbotClient
{
    // 表示聊天机器人的名称属性
    public string Name => "SparkDesk";

    // 可为空的字段，用于存储 SparkDesk 客户端实例
    SparkDeskClient? client;

    // 用于访问本地设置服务的字段，读取配置数据
    private readonly ILocalSettingsService _localSettingsService;

    // 构造函数，使用依赖注入获取本地设置服务
    public SparkDeskChatbotClient(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    // 异步方法，将用户的问题发送给聊天机器人并返回回复
    public async Task<string> AskQuestionResultAsync(string message)
    {
        // 从本地存储中读取聊天机器人配置
        var botSetting = await _localSettingsService.ReadSettingAsync<BotSetting>(Constants.BotSettingKey);
        if (botSetting == null)
        {
            // 如果未找到配置，则抛出异常，提示“配置为空”
            throw new Exception("配置为空");
        }

        // 如果客户端未初始化，则使用读取到的设置创建新的 SparkDesk 客户端实例
        client ??= new SparkDeskClient(
            botSetting.SparkDeskAppId,
            botSetting.SparkDeskAPIKey,
            botSetting.SparkDeskAPISecret);

        try
        {
            // 使用客户端异步发送聊天请求到 SparkDesk API
            ChatResponse response = await client.ChatAsync(ModelVersion.V3_5, new ChatMessage[]
            {
                // 创建包含用户输入的聊天消息
                ChatMessage.FromUser(message),
            }, new ChatRequestParameters
            {
                // 设置回复的最大长度为 200 个 token
                MaxTokens = 200,
                // 设置温度值控制随机性（0.5 为平衡响应）
                Temperature = 0.5f,
                // 设置 TopK 为 4，表示在生成回复时考虑前 4 种可能性
                TopK = 4,
            });

            // 使用调度队列安全地在 UI 线程上更新界面
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                // 显示回复文本的 Toast 消息，持续 10 秒
                ToastHelper.SendToast(response.Text, TimeSpan.FromSeconds(10));
            });

            // 返回聊天机器人的回复文本
            return response.Text;
        }
        catch (Exception ex)
        {
            // 如果在请求过程中发生错误，则捕获异常
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                // 显示错误信息的 Toast 消息，持续 10 秒
                ToastHelper.SendToast(ex.Message, TimeSpan.FromSeconds(10));
            });
        }

        // 如果发生错误，则返回空字符串
        return "";
    }
}
