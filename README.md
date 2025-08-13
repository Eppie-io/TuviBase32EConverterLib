# Base32EConverterLib
Base32EConverter can convert byte array into a string that can be an email name and vice versa.

Problem

You have a database/list of clients/employees/records etc. Each record have a specific field that represented as array of bytes. 
It cab ne identifier/public key/account number etc. And you have to create email address based on that specific field.

Our software solve this task. Email naming has some restrictions:
- Name's length should be no longer than 64 symbols;
- Email server should be case insensitive. So abc@domain.com, AbC@domain.com and ABC@domain.com are the same addresses;
- Name can not begin with symbol '.';
- Different email servers have diffrent additional restrictions on the use of special symbols such as <>()!+%$# and so on.

With the above in mind we chose next alphabet: abcdefghijkmnpqrstuvwxyz23456789
It's power 32 = 2^5 allows to use more simple and understandable algorithm of converting. Alphabet does not contain any special symbol or symbols that looks similar (such as o and 0 or 1 and l).

Algorithm

Array of bytes is treated as sequence of bits. We divide this sequence in groups of 5 bits from it's end. If the first group is shorter than 5 bits, consider it filled with leading zeros.
Then each group is changed to an according symbol of our alphabet. Finally we unite all symbols into a string. And vice versa.

For example, to convert byte[]={31,32,33} into a string, the converter represents array as a sequence of bits and divides it into groups of five bits (counting from the end). 

00011111(31), 00100000(32), 00100001(33) became

0001.11110.01000.00001.00001 and then

Groups are replaced by symbols: 00001 -> b, 01000 -> i, 11110 -> 8. So we get

b.8.i.b.b and string "b8ibb".

Also converter has possibility to do the same in opposite direction.

Code examples

```
byte[] array = new byte[]{31,32,33};
string result = Base32EConverter.ToEmailBase32(array);
// result = "b8ibb"

string name = "friend";
byte[] result = Base32EConverter.FromEmailBase32(name);
// result = { 10, 244, 17, 131 }
```
