<img alt="The Gnoam Gnome" src="http://idm.me.uk/media/img/gnoam.png" height="80"> Gnoam
=====

This document is a suggestion for a mini-language designed to specify
natural language generation content.

This repository may end up being added to with implementations,
depending on whether the project progresses, but until this message is
updated, you should assume anything else in this repo is merely
experimentation.

Simple Example
---

This example shows the basic generation method, but doesn't show any
of the data-driven features of the language.

    [root] -> [name] and [name] fell in love. [problem]
    [name] -> Abdul
    [name] -> Bethan
    [name] -> Carlos
    [name] -> Dai
    [problem; frequency 3] -> They lived [location-problem].
    [problem; frequency 2] -> Their parents [parent-problem].
    [problem] -> They started out hating each other.
    [location-problem] -> in distant cities
    [location-problem] -> on opposite sides of the country
    [location-problem] -> on different continents
    [parent-problem] -> disapproved of the match
    [parent-problem] -> were old enemies

This generates content such as:

> Abdul and Carlos fell in love. Their parents disapproved of the match.

or

> Bethan and Dai fell in love. They lived on different continents.

The advantage of Gnoam is that these choices can be data-driven and
can be stored for future use. So the fact that Bethan and Dai are on
different continents can affect the text generation later.

Language Features
---

### Feature-overload sample

This sample is not intended to be useful in any way, except to show as
many features as possible, so you can refer back to see the syntax.

    using gnoam/names as names
    using titles

    [root] -> [names.full-name] [action]

    [root] -> [military-title] [names.full-name] [action]

    # This is a comment line.

    [action] ->
        gave [person] [=kisses] kiss[= kisses; filter plural("", "es")].

    [root #tag1 #tag2; freq 2; if bob = 2; set bob += 1] ->
        [person #tag3; where char = bob; as char.name; filter drunk]

The first two lines import other files of rules. The rest of the
document defines more rules.

### Rules

Gnoam programs consist of sets of rules. Rules determine how a *tag*
is replaced by other content. To start with, we have only one tag,
`[root]` (the root node is just a convention, it is possible to begin
generation with any single tag). Rules may replace a tag with content
including other tags, which then need to be replaced in turn, until no
more tags remain.

Rules always match one tag. The `->` operator follows the tag, and
then the rest of the rule consists of the content to use in the
replacement. Rules can be broken onto multiple lines, with follow-on
lines indicated by indenting with whitespace.

Whitespace is trimmed from around replacement content and internal
whitespace is compressed into spaces (i.e. `/\s+/ /g`). This is
similar to the way HTML works. Explicit whitespace can be given with
backslash escapes: `\s` (a regular space), `\n` and `\t`.

If a tag cannot be replaced, it will be left in the final text and a
warning will be generated. If multiple rules match a tag, one will be
selected at random.

Gnoam files are unicode compatible and should be utf-8 encoded.

### Tag Names

Tag names do not contain whitespace and must begin with a letter
(unicode character categories beginning with `L` except `[Lm]`). They
conventionally use a hyphenated lower-case ASCII style, but this
preference is purely mine and isn't enforced.

A file defines a set of tags in its rules. Those rules can be accessed
from other files with the `using` directive at the start of the file.

* `using <path>` on its own imports a file's rules in to the current
  namespace as if they were defined here.

* `using <path> as <name>` imports rules under a common prefix,
  separated from the rest of the name by a period. So to refer to the
  `[name]` rule imported with `using foobar as foobar`, we'd use the
  tag `[foobar.name]`.

The periods are not reserved syntax, so it is possible to
define another rule with the tag `[foobar.name]` in the current file.

When the generation is run, a `GNOAM_RULE_PATH` variable is used to
find rule files. Directories in that variable are considered in
order. An imported name of the form `using foo/bar as name` looks in a
subdirectory `foo` for the rule file `bar.gnoam`.


### Special Tags


There are two special tags:

* `[]` And empty tag, always is replaced by nothing, but is used if
  you want to preserve whitespace at the start or end of
  content. (e.g. `[foo] -> [] [bar] []` will have a space around
  whatever bar is replaced by). Empty tags can also be used purely to
  hold clauses, in which case they begin with a semicolon.

* `[= <expression>]` Inserts the value of an expression, normally
  using data from the data object (see below).


### Clauses

Tags can have a number of additional *clauses* in them, which change
or augment their behavior. Clauses are semi-colon separated. There is
an optional semi-colon between the tag name and the first clause. For
empty tags, and insertion tags, this first semi-colon is required.

For example:

    [tag-name frequency 4; priority 2]
    [tag-name; frequency 4; priority 2]
    [; set variable = 3]
    [= variable; filter uppercase]


#### Clauses in rule definitions

These clauses can appear in the tag for a rule definition:

* `#hashtag #hashtag ...` One or more hashtags labeling the rule
  name. Hashtags have no whitespace and are case-insensitive. These
  can be specified by clauses in tags in the body of a rule too, in
  which case they provide an extra matching criteria. They are
  deliberately limited, however. For more comprehensive matching,
  using `if` clauses and store requirements in the data
  object. Although multiple hashtags can be given in one clause
  (i.e. not semi-colon separated), any number of hashtag clauses can
  be given (effectively this means that semi-colons between hashtags
  are optional).

* `freq <positive-number>` or `frequency <positive-number>` The
  relative frequency that this rule will be chosen when it matches. At
  most one of these clauses should be present.

* `pri <number>` or `priority <number>` When more than one rule is
  valid for a replacement, the priority values are considered. Only
  the highest priorities will be used. By default rules have
  priority 1. So adding a priority 0 rule with no `if` or other
  matching criteria is a good way to make a fall-back rule that
  generates replacement if not better alternative exists.

* `if <boolean-expression>` A condition which must be fulfilled for
  the rule to be chosen. Any number of these clauses can be present
  and all must pass before the rule is chosen.

#### Clauses in tags in rule bodies

* `<hashtag expression>` An expression containing hashtags and boolean
  operators (`and`, `not`, `or` and parentheses). This limits the
  rules that will be used to fulfill this tag to those that match the
  expression. There is an implied `and` between adjacent hashtags, so
  `#foo #bar or #sun` is equivalent to `#foo and #bar or #sun`.

#### Clauses in tags in either place.

All these clauses can be repeated.

* `set <data-path> <operator>? <expression>?` Sets some data value
  before replacing the tag. See below for details.

* `where <data-path> <operator>? <expression>?` Sets some data value
  while replacing the tag, but reverts its value afterwards.

* `as <data-path>` Takes the final replacement text and assigns it to
  some data value.

* `filter <filter>(<param>...)` Takes the replacement text and passes
  it through the given filter. This can change the output. Parameters
  depend on the filter, and if a filter has no parameters, then the
  parentheses after the filter name are optional.

These clauses are order-dependent. Filters can be applied before or
after the replacement content is saved. All `where` clauses are
reverted after these clauses are processed, in the reverse order that
they were set. The order of `set` and `where` clauses are honored.

Further refinement of the order of setting values can be achieved by
moving set clauses out into empty tags. So to set the value after a
tag is processed, you can do this:

    [tag-to-replace][;set foo = 2]

### Data

Gnoam is data driven. Generation begins with a root node and a
namespace of data. That data can be queried and modified during text
generation.

Data can consist of numbers (floating point), strings of text, or
objects. Objects are Lua-like: they represent both name-to-value
mappings and lists of data.

Numeric values can be given as literals. Literal strings of text can be
enclosed within single or double quotation marks.

    set name = "Bob"

Strings can have escape sequences for whitespace and quotation marks.

And object literal can be given using curly braces with items
separated by commans and key-to-value pairs separated by a colon:

    set foo = {bar: 4}

The empty object can be given with `{}` or the `new` keyword:

    set foo = new
    set foo = {}

And object is also created by assigning to a field within it. So if
`foo` doesn't exist yet:

    set foo.bar = 4

creates `foo` as an object and sets its `bar` property. It is equivalent of

    set foo = {bar: 4}


#### Boolean values and flags

Numbers are used to represent boolean values. Where zero is false and
anything else is true. To ease the common case where variables just
represent whether something has been done or seen (often called 'flag'
variables), `set` and `where` can be given just a variable name, and
they set the value to 1. For example

    [tag set variable]
    [tag set variable 1]
    [tag set variable = 1]

are equivalent.

Similarly in `if` expressions, a variable used in a boolean expression
will be false if the variable is not defined, if it is the number
zero, if it is the empty string, or if it is the empty object (these
are sometimes called 'falsy' values). All other values are treated as
true (they are said to be 'truthy' values).

So we can test for the variable above in a rule with

    [another-tag if variable] -> ...


#### Operators

When setting data, the `=` operator is optional, so the above could be

    set foo new

Other operators are available:

* `+` or `+=` adds the expression to the data value. If the value is a
   number this is numeric addition. If it is a string it is
   concatenation. If it is an object it adds it to the end of the
   list. If the value doesn't exist it is equivalent to the `=`
   operator.

* `-`, or `-=` subtracts the expression from the value. Again it is
  `=` if the value doesn't exist. If the value being changed is an
  object, it tries to remove the value from the list. It is an error
  for strings.

* `*` or `*=` multiplies the value by expression. Numerically it
  behaves as above. For lists or strings it concatenates multiple
  repeats.

* `/`, or `/=` divides the value by the expression. They are only
  defined for numeric or missing values.

* `%` or `%=` performs modular division on numeric values.

* `^` or `^=` performs exponentiation.

* `++` or `++=` performs the union of objects, or the concatenation of
  their list data. The data value may be missing or otherwise must be
  an object. The expression must be an object.

* `?=` sets the value only if it is not already set.

* `>=` makes sure a numeric value is no less than the expression.

* `<=` makes sure a numeric value is no more than the expression.

The latter two imply that limiting in a range can be done in two clauses:

    set foo >= 0; set foo <= 10

makes sure `foo` is a number between 0 and 10 inclusive.

Conclusion
---

The aim of Gnoam is to be relatively simple for simple cases

    [root] -> We have a [large-description] range of products.

    [large-description] -> huge
    [large-description] -> vast
    [large-description] -> massive

flexible for data-oriented applications

    [root] -> Your portfolio has experienced [change-description].

    [change-description if 5 < change] -> dramatic gains
    [change-description if 0 < change <= 5] -> modest gains
    [change-description priority 0] -> challenging circumstances

and scalable to more complex tasks

    [root] ->
        This is the story of how I fell in love with
        [first-name as prot.first-name] [last-name as prot.last-name].
        [= prot.first-name] was the first person I met in
        [city as location.city].

Name
---

The name is a play on the first name of Noam Chomsky, who's theory of
syntax is so central to this project, and most of the rest of natural
language AI.
