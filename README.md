GDDL: General-purpose Data Description Language
=============

GDDL is a library for accessing data files described in the GDDL syntax.

The source code is licensed under the 3-clause BSD license.
See [LICENSE.txt](/LICENSE.txt) for details.

A complete example
--------------------

```
RootSetName = typeNameHere {

    # Basic elements

    null, nil,
    false, true,
    12345,
    0x12345,
    123.45,
    123e+45,
    .23,
    .23e45,
    12.34e-5,
    "This is a string literal",
    "Testing \t\f\b\r\n escape\x20codes\x1234",
    'Strings can also be single-quoted.',

    { 1,2,3,4,5 } # the comma is optional after a closing brace

    # Named elements

    namedNumber = 12345,
    namedString = "12345",

    # Named sets

    namedSet = { "a", { 1 }, 0x345 },
    namedTypedSet = set_with_a_type { "\x0001" }
    
    # References
    
    replace_this_with = RootSetName:namedNumber,

    # The comma in the last element is optional but allowed.
}
```

Syntax
-------

Literals:

* Nil: ```nil``` or ```null```
* Boolean: ```true``` and ```false```
* Identifier: ```[a-zA-Z_][a-zA-Z_0-9]*```
* Integer: ```[0-9]+```
* HexInt: ```0x[0-9a-fA-F]*```
* Decimal: ```[0-9]*.[0-9]+```
* Scientific(1): ```[0-9]*.[0-9]+e[+-]?[0-9]+```
* Scientific(2): ```[0-9]+e[+-]?[0-9]+```
* String(1): ```\"([^"\\]|(\\[tbrn])|(\\x[0-9a-fA-F]{1,4}))*\"```
* String(2): ```\'([^'\\]|(\\[tbrn])|(\\x[0-9a-fA-F]{1,4}))*\'```

Pseudo-BNF rules:

```
Root: Element

Element: Basic-Element | Named-Element

Named-Element: Identifier '=' Basic-Element

Basic-Element: Literal | Set | Reference

Literal: Integer | Decimal | Scientific | String

Set: Identifier? '{' Element-List '}'

Element-List: (Element (COMMA¹ Element)*)?
    ¹: Optional if the previous Element was a Set

Reference: Identifier (':' Identifier)*
```

"What did I just read?", or, "Syntax the nice and easy way"
--------------------------------------------------

Numeric literals and strings work mostly as you know them from languages such as C or Java.

At the root of the document, is an element. This element can be of any type and fashion,
but the language is most effective when the root is a (optionally named and/or typed) set.

An element in the language can be a literal (numeric or string), or a set.

An element can optionally be named, that is, in the form "name = element",
which gives you (writer or consumer both) the ability to look up an element by name in the hierarchy.

A set is a (ordered) collection of elements, akin to a list, but with the added feature of
being able to look up named elements based on their name.

A set can also be typed. Unlike names, there can be multiple elements with the same type.

The type does not matter to the parser, but it could be used in a schema file
to provide a predetermined set of rules for verification purposes.

(No there's no schema system defined yet for this language feel free to contribute one.)

A set also has the ability to look up typed elements by their type name, and return the subset.

Finally, the syntax allows for referencing other elements in the hiererchy,
effectively allowing the file to describe a graph instead of just a tree.

In the current version, references work solely on names, and can not reference elements by their type.

Potential future improvements (some crazier than others)
------------------------------

1. Allow references by type, index within the parent set, and/or range selectors.
  * Tentative syntax: Root:[3]:typename1[*]:typename2[2..3]

1. Allow more fine-grained typing.
  * Tentative syntax: 1234ui64 --> unsigned integer of 64bits.

1. Templating support: element substitution engine.
  * Basically turns the system into effectively a lambda-calculus engine!
  * Syntax to be determined.
