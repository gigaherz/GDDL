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
    "Testing \t\f\b\r\n escape\x20codes\u1234",
    'Strings can also be single-quoted.',

    { 1,2,3,4,5 } # the comma is optional after a closing brace

    # Named elements

    namedNumber = 12345,
    namedString = "12345",

    # Named sets

    "named collection" = { "a", { 1 }, 0x345 },
    namedTypedSet = set_with_a_type { "\u0001" }

    # References

    replace_this_with = RootSetName:namedNumber,

    # The comma in the last element is optional but allowed.
}

```

Syntax
-------

Literals:

* nil: ```nil``` or ```null```
* boolean: ```true``` and ```false```
* identifier: ```[a-zA-Z_][a-zA-Z_0-9]*```
* integer: ```[0-9]+```
* hex-integer: ```0x[0-9a-fA-F]*```
* decimal: ```[0-9]*.[0-9]+```
* scientific(1): ```[0-9]*.[0-9]+e[+-]?[0-9]+```
* scientific(2): ```[0-9]+e[+-]?[0-9]+```
* string(1): ```\"([^"\\]|(\\[tbrn])|(\\x[0-9a-fA-F]{1,4}))*\"```
* string(2): ```\'([^'\\]|(\\[tbrn])|(\\x[0-9a-fA-F]{1,4}))*\'```

eBNF rules:

```ebnf
root = element ;

element = named-element
        | basic-element  
        ;

name = identifier | string ;

named-element = name, '=', basic-element ;

basic-element = value | collection | reference ;

reference = identifier-list
          | ':' identifier-list
          ;

identifier-list = identifier
                | identifier ':' identifier-list
                ;

collection = [ identifier ], '{', element-list, '}' ;

element-list = element, [ ',' ]
             | element, ',', element-list
             | collection, element-list
             ;

value = integer | hex-integer | decimal | scientific | string ;

```

"What did I just read?", or, "Syntax the nice and easy way"
--------------------------------------------------

Numeric literals and strings work mostly as you know them from languages such as C or Java.

At the root of the document, is an element. This element can be of any type and fashion,
but the language is most effective when the root is a (optionally named and/or typed) collection.

An element in the language can be a literal (numeric or string), a collection, or a reference.

An element can optionally be named, that is, in the form "name = element",
which gives you (writer or consumer both) the ability to look up an element by name in the hierarchy.

A collection is an ordered sequence of elements, akin to a list, but with the added feature of
being able to look up named elements based on their name.

Elements in a collection are separated by commas. The comma is optional after a nested collection.
The last element in a collection can have a comma.

A collection can also be typed. Unlike names, there can be multiple elements with the same type.

The type does not matter to the parser, but it could be used in a schema file
to provide a predetermined collection of rules for verification purposes.

(No there's no schema system defined yet for this language feel free to contribute one.)

A collection also has the ability to look up typed elements by their type name, and return the subset.

Finally, the syntax allows for referencing other elements in the hierarchy,
effectively allowing the file to describe a graph instead of just a tree.

In the current version, references work solely on names, and can not reference elements by their type.

It looks a lot like JSON, why not just use JSON?
------------------------------------------------

If you were already wondering that when you started reading, then why are you even here? Just use JSON, it's a perfectly fine language. I'm not trying to take it away from you.

To anyone else: I wrote this before I learned JSON. It's designed to look a bit like the syntax of C-style programming languages, without the verbosity of XML. This overlaps a lot with the design of JSON, but it's intrinsically different.

How to Use
--------------------

In your maven/gradle/ivy file, include `https://dl.bintray.com/gigaherz/maven` as a maven repository, then use the groupId `gigaherz.util.gddl`, artifactId `gigaherz.util.gddl2`, and the version you want to use.

Use the `Parser.fromFile` function to initialize a parser, and then call `parse` on the parser object to obtain the high-level representation of the data.

Potential future improvements (some crazier than others)
------------------------------

1. Allow references by type, index within the parent collection, and/or range selectors.
   * Tentative syntax: Root:[3]:typename1[*]:typename2[2..3]

1. Allow more fine-grained typing.
   * Tentative syntax: 1234ui64 --> unsigned integer of 64bits.

1. Templating support: element substitution engine.
   * Basically turns the system into effectively a lambda-calculus engine!
   * Syntax to be determined, but probably along the lines of:
     ```
something = typeName ( name1, name2, name3, ... ) { 
... references to name2/name2/name3 ... 
}
something(value1, value2, value3)
something(name1=value1, name2=value2, name3=value3)
```