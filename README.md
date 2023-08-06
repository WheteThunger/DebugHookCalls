## What problem does this solve?

Ever see warnings in console about plugins using a lot of "average" hook time? For example:

```
Calling 'OnItemSplit' on 'StackModifier v1.7.1' took average 1822ms
Calling 'CanStackItem' on 'StackModifier v1.7.1' took average 1975ms
Calling 'CanStackItem' on 'StackModifier v1.7.1' took average 2057ms
Calling 'CanStackItem' on 'StackModifier v1.7.1' took average 1975ms
Calling 'OnItemSplit' on 'StackModifier v1.7.1' took average 1870ms
Calling 'OnItemSplit' on 'StackModifier v1.7.1' took average 1864ms
Calling 'OnItemSplit' on 'StackModifier v1.7.1' took average 1866ms
Calling 'OnItemSplit' on 'StackModifier v1.7.1' took average 2127ms
Calling 'OnItemAddedToContainer' on 'StackModifier v1.7.1' took average 1962ms
Calling 'CanStackItem' on 'StackModifier v1.7.1' took average 2098ms
```

This typically means that something in your server is triggering a very high quantity of hook calls, overloading plugins that are subscribed to those hooks, and causing server lag in the process. This is most commonly seen with item-related hooks and is caused by complex industrial systems. There is little that plugins can do to mitigate the performance cost of hooks being called thousands of times per second, especially in the short term.

If you have seen this issue, you may have tried removing plugins or replacing plugins that are named in the warnings. Unfortunately, that can be disruptive to players, as well as cost you time and money. Fortunately, in many cases, you may be able to use this plugin to quickly identify the source of the problem on your server so that you can disable the industrial system causing it.

## What should I do?

Download and install DebugHookCalls.cs into your plugins folder, then run one of the following commands to count how many times a specific hook is being called in a 10 second time period.

```
debughookcalls.start CanAcceptItem 10
debughookcalls.start CanStackItem 10
debughookcalls.start OnItemAddedToContainer 10
debughookcalls.start OnItemSplit 10
debughookcalls.start OnMaxStackable 10
```

Note: The hook names shown above (CapAcceptItem, CanStackItem, etc...) are the only hooks supported by this plugin at this time. More hooks can be added on request, but it's unlikely that you will need others, since complex industrial systems will trigger multiple item related hooks, so you only need to monitor one of them to identify where the issue is happening.

After running one of those commands, a report will be printed in the server console 10 seconds later like the following. It will show the total call count for that hook, and the top locations.

``` 
[Debug Hook Calls] Hook CanAcceptItem was called 142325 times over 10 second(s)
[Debug Hook Calls] 64509 calls at approximate location (-300.0, 0.0, 500.0)
[Debug Hook Calls] 54341 calls at approximate location (200.0, 0.0, -400.0)
[Debug Hook Calls] 231 calls at approximate location (-600.0, 0.0, -500.0)
[Debug Hook Calls] 56 calls at approximate location (-400.0, 0.0, -300.0)
[Debug Hook Calls] 11 calls at approximate location (500.0, 0.0, 400.0)
```

Next, go in-game to the top locations (the ones with calls in the thousands), look for complex industrial systems inside player bases, and disable those industrial systems (e.g., disconnect their power, or disconnect the industrial pipes). Then, run the command again, to see if the call count has decreased. If it did, you should also stop seeing warnings in console about average hook time, server lag will have subsided, and server fps will have increased.
