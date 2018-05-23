﻿## Alerts
Alerts are boxes within articles that contain tangential content. Such content can be things like extra information and warnings. Alerts have a similar syntax to 
blockquotes. However, they have very different purposes - according to the [specifications](https://html.spec.whatwg.org/multipage/grouping-content.html#the-blockquote-element)
blockquotes should be used when quoting from external articles.

Every line of an alert must start with an `!`. The first line of an alert must be of the form `!<optional space><alert name>` where `<alert name>`
is a string containing 1 or more characters from the regex character set `[A-Za-z0-9_-]`. The result of appending `alert-` to the alert name is used as the
alert block's class:

```````````````````````````````` example
! critical-warning
! This is a critical warning.
.
<div class="alert-critical-warning">
<p>This is a critical warning.</p>
</div>
````````````````````````````````

The block is ignored if the first line does not contain a level name :

```````````````````````````````` example
! 
! This is a warning.
.
<p>!
! This is a warning.</p>
````````````````````````````````

The block is ignored if the first line contains disallowed characters :

```````````````````````````````` example
! illegal space
! This is a warning.
.
<p>! illegal space
! This is a warning.</p>
````````````````````````````````

The first space after `!` is ignored. :

```````````````````````````````` example
! warning
!This line will be rendered with 0 leading spaces.
! This line will also be rendered with 0 leading spaces.
.
<div class="alert-warning">
<p>This line will be rendered with 0 leading spaces.
This line will also be rendered with 0 leading spaces.</p>
</div>
````````````````````````````````

Lazy continuation is allowed:

```````````````````````````````` example
! warning
! This is part of
the warning.
! This is also part of
the warning.
.
<div class="alert-warning">
<p>This is part of
the warning.
This is also part of
the warning.</p>
</div>
````````````````````````````````

`AlertsExtensionOptions.IconMarkups` can be used to define icon element markup for alert types:

```````````````````````````````` options
{
    "alerts": {
        "iconMarkups": {
            "warning": "<svg><use xlink:href=\"#warning-icon\"></use></svg>",
            "information": "<svg><use xlink:href=\"#information-icon\"></use></svg>"
        }
    }
}
```````````````````````````````` example
! warning
! This is a warning.

! information
! This is information.
.
<div class="alert-warning">
<svg><use xlink:href="#warning-icon"></use></svg>
<div class="alert-content">
<p>This is a warning.</p>
</div>
</div>
<div class="alert-information">
<svg><use xlink:href="#information-icon"></use></svg>
<div class="alert-content">
<p>This is information.</p>
</div>
</div>
````````````````````````````````

Per-alert-block options can be overriden if the JSON options extension is enabled:

```````````````````````````````` options
{
    "alerts": {
        "iconMarkups": {
            "warning": "<svg><use xlink:href=\"#warning-icon\"></use></svg>"
        }
    }
}
```````````````````````````````` example
! warning
! This is a warning.
@{
    "iconMarkup": "<svg><use xlink:href=\"#special-warning-icon\"></use></svg>"
}
! warning
! This is a special warning.
.
<div class="alert-warning">
<svg><use xlink:href="#warning-icon"></use></svg>
<div class="alert-content">
<p>This is a warning.</p>
</div>
</div>
<div class="alert-warning">
<svg><use xlink:href="#special-warning-icon"></use></svg>
<div class="alert-content">
<p>This is a special warning.</p>
</div>
</div>
````````````````````````````````