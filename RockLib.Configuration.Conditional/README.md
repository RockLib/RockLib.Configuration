# RockLib.Configuration.Conditional

Augment a `IConfigurationSection` by merging in an override section, selected by
a config property.

## Example

Suppose your application tracks production of widgets, using configuration like this:

```yaml
WidgetService:
    DailyQuota: 100
    Workers: 4
    WidgetColor: blue
```

The configuration is mapped into a binding class.

```csharp
public class WidgetServiceConfiguration
{
    public int DailyQuota { get; set; }
    public int Workers { get; set; }
    public string WidgetColor { get; set; }
}
```

Using dependency injection, we can make this configuration available to our application.

```csharp
services.Configure<WidgetServiceConfiguration>(Configuration.GetSection("WidgetService"));
```

### Supporting conditional configuration

During holiday season, production must increase to support demand. We can use
the `SwitchingOn(property)` extension method to create a `IConfigurationSection`
that will merge in a sub-section of configuration based on a new `Season`
property.

```csharp
services.Configure<WidgetServiceConfiguration>(Configuration.GetSection("WidgetService").SwitchingOn("Season"));
```

Any properties that change depending on the season are duplicated into
sub-sections corresponding to each season.

```yaml
WidgetService:
    Season: Normal
    Normal:
        DailyQuota: 100
        Workers: 4
    Holiday:
        DailyQuota: 500
        Workers: 15
    WidgetColors: blue
```

The resulting configuration would look like this:

```yaml
WidgetService:
    Season: Normal
    Normal:
        DailyQuota: 100
        Workers: 4
    Holiday:
        DailyQuota: 500
        Workers: 15
    WidgetColors: blue

    # Properties merged in from the 'Normal' sub-section
    DailyQuota: 100
    Workers: 4
```

Since we're binding this config into the `WidgetServiceConfiguration` class,
only the relevant properties are used:

```yaml
WidgetService:
    DailyQuota: 100
    Workers: 4
    WidgetColor: blue
```

For the holiday season, we simply change the `Season` property, and the
appropriate sub-section is merged in.

```yaml
WidgetService:
    Season: Holiday
    Normal:
        DailyQuota: 100
        Workers: 4
    Holiday:
        DailyQuota: 500
        Workers: 15
    WidgetColors: blue

    # Properties merged in from the 'Holiday' sub-section
    DailyQuota: 500
    Workers: 15
```

This leaves these relevant properties for binding into `WidgetServiceConfiguration`:

```yaml
WidgetService:
    DailyQuota: 500
    Workers: 15
    WidgetColor: blue
```
