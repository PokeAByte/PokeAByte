Issues found while writing tests:

1. When setting a property from JavaScript, the value is not cast to the property target type.

This also means that it's possible to write a string to an `int` property.

2. The `int` type behaves slightly incorrectly on small lengths. 

For example `0xFF` is `255` instead of `-127` on an int of length = 1.


