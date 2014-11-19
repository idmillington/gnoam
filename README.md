Gnoam
=====

This document is a suggestion for a mini-language designed for
technical end-users to specify natural language generation
content. The repository may end up being added to with
implementations, depending on whether the project progresses, but
until this message is updated, you should assume anything else in this
repo is merely experimentation.

Feature Demonstration Example
---

This sample is not intended to be useful in any way, but just to pack
in as many features as possible.

    using gnoam.names as names
    using titles

    [root] -> [names.full-name] [action]

    [root] -> [military-title] [names.full-name] [action]

    # This is a comment line.

    the * [action] /regex/ ->
        gave [person] [=kisses] kiss[= kisses; filter plural("", "es")].

    [root #tag1 #tag2; freq 2; if bob = 2; set bob += 1] ->
        [person #tag3; where char = bob; as char.name; filter drunk]

The first two lines import other sets of rules. The rest of the
document defines more rules.

Items written as `[root]` are called *tags*. Rules have one tag,
possibly surrounded by other content (including wildcards and regular
expressions), followed by the `->` operator, then some content to
replace the tag by.

The replacement content can include further tags, which will in turn
be replaced, until no more tags remain and the final text is
generated.

Whitespace is trimmed from around replacement content and internal
whitespace is compressed into spaces (i.e. `/\s+/ /g`). Explicit
whitespace can be given with backslash escapes: `\n` and `\t`.

If a tag cannot be replaced, it will be left in the final text and a
warning will be generated.

If multiple rules match a tag, one will be selected at random.

Tag Names
---

Tags do not contain whitespace. They conventionally use a hyphenated
lower-case style.

A file defines a set of tags in its rules. Those rules can be accessed
from other files with the `using` directive at the start of the file.

- `using <path>` on its own brings a files' rules in to the current
  namespace as if they were defined here.

- `using <path> as <name>` brings rules in, under a common prefix,
  separated from the rest of the name by a period. So to refer to the
  `[name]` rule in the `foobar` file, we'd use the tag
  `[foobar.name]`.

The periods are not reserved syntax, so it is totally possible to
define another rule with the tag `[foobar.name]` in the current file.


Special Tags
---

There are two special tags:

- `[]` And empty tag, always is replaced by nothing, but is used if
  you want to preserve whitespace at the start or end of
  content. (e.g. `[foo] -> [] [bar] []` will have a space around
  whatever bar is replaced by).

- `[= <expression>]` Inserts the value of an expression, normally
  using data from the data object (see below).

Clauses
---

Tags can have a number of additional *clauses* in them, which change
or augment their behavior. A tag can have a single clause following
the tag name, and further clauses are semi-colon separated.

### Clauses in rule definitions

These clauses can appear in the tag for a rule definition:

- `#hashtag #hashtag ...` One or more hashtags labeling the rule
  name. Hashtags have no whitespace and are case-insensitive. These
  can be specified by clauses in tags in the body of a rule too, in
  which case they provide an extra matching criteria. They are
  deliberately limited, however. For more comprehensive matching,
  using `if` clauses and store requirements in the data
  object. Although multiple hashtags can be given in one clause
  (i.e. not semi-colon separated), any number of hashtag clauses can
  be given (effectively this means that semi-colons between hashtags
  are optional).

- `freq <positive-number>` The relative frequency that this rule will
  be chosen when it matches. At most one of these clauses should be
  present.

- `priority <number>` When more than one rule is valid for a
  replacement, the priority values are considered. Only the highest
  priorities will be used. By default rules have priority 1. So adding
  a priority 0 rule with no `if` or other matching criteria is a good
  way to make a fallback rule that generates replacement if not better
  alternative exists.

- `if <boolean-expression>` A condition which must be fulfilled for
  the rule to be chosen. Any number of these clauses can be present
  and all must pass before the rule is chosen.

- `set <data-path> <operator>? <expression>` Sets some data value
  before performing the replacement, if this rule is selected. See
  below for details. Any number of such clauses can be present.

- `set-after <data-path> <operator>? <expression>` Sets some data value
  before after the replacement, if this rule is selected. See
  below for details. Any number of such clauses can be present.

### Clauses in tags in rule bodies

All these clauses can be repeated.

- `<hashtag expression>` An expression containing hashtags and boolean
  operators (`and`, `not`, `or` and parentheses). This limits the
  rules that will be used to fulfil this tag to those that match the
  expression. There is an implied `and` between adjacent hashtags, so
  `#foo #bar or #sun` is equivalent to `#foo and #bar or #sun`.

- `if <boolean-expression>` A condition which must be fulfilled for
  this tag to be replaced. If this fails, the tag will be
  removed. This is not designed to have `else` logic: a set of tags
  one of which is chosen. For that, use a tag with multiple possible
  rules.

- `set <data-path> <operator>? <expression>` Sets some data value
  before replacing the tag. See below for details.

- `set-after <data-path> <operator>? <expression>` Sets some data value
  before replacing the tag. See below for details.

- `where <data-path> <operator>? <expression>` Sets some data value
  while replacing the tag, but reverts its value afterwards.

- `as <data-path>` Takes the final replacement text and assigns it to
  some data value.

- `filter <filter>(<param>...)` Takes the replacement text and passes
  it through the given filter. This can change the output. Parameters
  depend on the filter, and if a filter has no parameters, then the
  parentheses after the filter name are optional.

The `set-after`, `as` and `filter` clauses are
order-dependent. Filters can be applied before or after the
replacement content is saved. All `where` clauses are reverted after
these clauses are processed, in the reverse order that they were set.

The order of `set` and `where` clauses are honored.

Data
---

Gnoam is data driven. Generation begins with the `[root]` node (by
convention, any can be specified) and a namespace of data. That data
can be queried and modified during text generation.

Data can consist of numbers (floating point), strings of text, or
objects. Objects are Lua-like: they represent both name to value
mappings and lists of data. An object is created by either assigning
to a field within it, using the `new` keyword, or using curly braces
as an object literal. E.g. if `foo` doesn't exist yet:

    set foo.bar = 4

creates `foo` as an object and sets its `bar` property. As does

    set foo = {bar: 4}

Where

    set foo = new

creates `foo` as an empty object.

### Operators

When setting data, the `=` operator is optional, so the above could be

    set foo new

Other operators are available:

- `+` or `+=` adds the expression to the data value. If the value is a
  number this is numeric addition. If it is a string it is
  concatenation. If it is an object it adds it to the end of the
  list. If the value doesn't exist it is equivalent to the `=`
  operator.

- `-`, or `-=` subtracts the expression from the value. Again it is
  `=` if the value doesn't exist. If the value being changed is an
  object, it tries to remove the value from the list. It is an error
  for strings.

- `*` or `*=` multiplies the value by expression. Numerically it
  behaves as above. For lists or strings it concatenates multiple
  repeats.

- `/`, or `/=` divides the value by the expression. They are only
  defined for numeric or missing values.

- `%` or `%=` performs modular division on numeric values.

- `^` or `^=` performs exponentiation.

- `++` or `++=` performs the union of objects, or the concatenation of
  their list data. The data value may be missing or otherwise must be
  an object. The expression must be an object.

- `?=` sets the value only if it is not already set.

- `>=` makes sure a numeric value is no less than the expression.

- `<=` makes sure a numeric value is no more than the expression.

The latter two imply that limiting in a range can be done in two clauses:

    set foo >= 0; set foo <= 10

makes sure `foo` is a number between 0 and 10 inclusive.

Conclusion
---

The aim of Gnoam is to be a relatively simple syntax for simple cases

    [root] -> We have a [large-description] range of products.

    [large-description] -> huge
    [large-description] -> vast
    [large-description] -> massive

flexible for data-oriented applications

    [root] -> Your portfolio has experienced [change-description].

    [change-description if 5 < change] -> dramatic gains
    [change-description if 0 < change <= 5] -> modest gains
    [change-description priority 0] -> challenging circumstances

and ready for more complex tasks

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
