using Oxide.Core.Libraries.Covalence;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Debug Hook Calls", "WhiteThunder", "0.1.0")]
    [Description("Debug hooks that are being called frequently")]
    public class DebugHookCalls : CovalencePlugin
    {
        #region Fields

        private const float RoundFactor = 100;
        private const int MaxLocationsToPrint = 5;

        private string _activeHook;
        private float _testDuration;
        private int _callCount;
        private Timer _timer;
        private Dictionary<Vector3, int> _callCountByRegion = new Dictionary<Vector3, int>();

        private string[] _hookNames =
        {
            nameof(CanAcceptItem),
            nameof(CanStackItem),
            nameof(OnItemAddedToContainer),
            nameof(OnItemSplit),
            nameof(OnMaxStackable),
        };

        #endregion

        #region Hooks

        private void Init()
        {
            foreach (var hookName in _hookNames)
            {
                Unsubscribe(hookName);
            }
        }

        private void CanAcceptItem(ItemContainer container, Item item, int targetPosition) => HandleHookCall(container);
        private void CanStackItem(Item hostItem, Item movedItem) => HandleHookCall(hostItem?.parent);
        private void OnItemAddedToContainer(ItemContainer container, Item item) => HandleHookCall(container);
        private void OnItemSplit(Item sourceItem, int amount) => HandleHookCall(sourceItem?.parent);
        private void OnMaxStackable(Item item) => HandleHookCall(item?.parent);

        #endregion

        #region Commands

        [Command("debughookcalls.start")]
        private void CommandStart(IPlayer player, string cmd, string[] args)
        {
            if (!player.IsServer)
                return;

            float duration;
            if (args.Length < 2 || !float.TryParse(args[1], out duration))
            {
                player.Reply($"Usage: {cmd} <hook name> <seconds>");
                return;
            }

            var hookName = args[0];
            if (!_hookNames.Contains(hookName))
            {
                player.Reply($"Unsupported hook name: {hookName}");
                return;
            }

            if (_activeHook != null && _timer != null && !_timer.Destroyed)
            {
                player.Reply($"Stopped current test for hook {_activeHook}");
                StopTest();
            }

            StartTest(hookName, duration);
        }

        #endregion

        #region Helpers

        private void HandleHookCall(ItemContainer container = null)
        {
            _callCount++;

            var entityOwner = container?.entityOwner;
            if (entityOwner == null)
                return;

            var pos = container.entityOwner.transform.position;
            var roundedPos = new Vector3(
                Mathf.Round(pos.x / RoundFactor) * RoundFactor,
                Mathf.Round(pos.y / RoundFactor) * RoundFactor,
                Mathf.Round(pos.z / RoundFactor) * RoundFactor
            );

            int count;
            if (!_callCountByRegion.TryGetValue(roundedPos, out count))
            {
                count = 0;
            }

            _callCountByRegion[roundedPos] = count + 1;
        }

        private void StartTest(string hookName, float duration)
        {
            _activeHook = hookName;
            _testDuration = duration;
            _callCount = 0;
            _timer = timer.Once(duration, StopTestAndReport);
            _callCountByRegion.Clear();
            Subscribe(hookName);

            LogWarning($"Subscribing to hook {hookName} for {duration} second(s)");
        }

        private void StopTest()
        {
            Unsubscribe(_activeHook);

            _activeHook = null;
            _timer?.Destroy();
            _timer = null;
        }

        private void StopTestAndReport()
        {
            LogWarning($"Hook {_activeHook} was called {_callCount} times over {_testDuration} second(s)");

            if (_callCountByRegion.Count > 0)
            {
                var sorted = _callCountByRegion.OrderByDescending(entry => entry.Value).Take(MaxLocationsToPrint).ToList();
                foreach (var entry in sorted)
                {
                    LogWarning($"{entry.Value} calls at approximate location {entry.Key}");
                }
                _callCountByRegion.Clear();
            }

            StopTest();
        }

        #endregion
    }
}
