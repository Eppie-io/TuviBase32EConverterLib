using NUnit.Framework;
using Tuvi.Base32EConverterLib;

namespace Tuvi.Base32EConverterTests
{
    [TestFixture]
    public class Base32EConverterIsEmailBase32Tests
    {
        [Test]
        public void IsEmailBase32_NullString_ReturnsFalse()
        {
            string input = null;
            var result = Base32EConverter.IsEmailBase32(input);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsEmailBase32_EmptyString_ReturnsFalse()
        {
            var result = Base32EConverter.IsEmailBase32(string.Empty);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsEmailBase32_WhitespaceOnly_ReturnsFalse()
        {
            var result = Base32EConverter.IsEmailBase32("   ");
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsEmailBase32_ValidLowercaseAlphabet_ReturnsTrue()
        {
            var alphabet = "abcdefghijkmnpqrstuvwxyz23456789"; // full alphabet
            var result = Base32EConverter.IsEmailBase32(alphabet);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsEmailBase32_ValidUppercaseAlphabet_CaseInsensitive_ReturnsTrue()
        {
            var alphabetUpper = "ABCDEFGHJKMNPQRSTUVWXYZ23456789"; // excluded letters already removed
            var result = Base32EConverter.IsEmailBase32(alphabetUpper); // default caseInsensitive = true
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsEmailBase32_ValidMixedCase_CaseInsensitive_ReturnsTrue()
        {
            var value = "AbcDefGhJkMnPqRsTuVwXyZ23456789"; // mixed case using allowed letters
            var result = Base32EConverter.IsEmailBase32(value);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsEmailBase32_ContainsExcludedLowercaseL_ReturnsFalse()
        {
            var value = "abcl"; // 'l' not in alphabet
            var result = Base32EConverter.IsEmailBase32(value);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsEmailBase32_ContainsExcludedUppercaseO_ReturnsFalse()
        {
            var value = "ABCO"; // 'O' not in alphabet, should fail even case-insensitive because 'o' excluded
            var result = Base32EConverter.IsEmailBase32(value);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsEmailBase32_ContainsPunctuation_ReturnsFalse()
        {
            var value = "abc!";
            var result = Base32EConverter.IsEmailBase32(value);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsEmailBase32_CaseSensitive_UppercaseInput_ReturnsFalse()
        {
            var value = "ABC"; // uppercase valid letters but caseInsensitive = false
            var result = Base32EConverter.IsEmailBase32(value, caseInsensitive: false);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsEmailBase32_CaseSensitive_LowercaseInput_ReturnsTrue()
        {
            var value = "abc";
            var result = Base32EConverter.IsEmailBase32(value, caseInsensitive: false);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsEmailBase32_CaseSensitive_MixedCaseInput_ReturnsFalse()
        {
            var value = "aBc";
            var result = Base32EConverter.IsEmailBase32(value, caseInsensitive: false);
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsEmailBase32_DigitsBoundary_ReturnsTrue()
        {
            var value = "23456789"; // all allowed digits
            var result = Base32EConverter.IsEmailBase32(value);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsEmailBase32_LengthGreaterThan64_AllValid_ReturnsTrue()
        {
            var value = new string('a', 65); // method does not enforce max length
            var result = Base32EConverter.IsEmailBase32(value);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsEmailBase32_LengthGreaterThan64_WithInvalidChar_ReturnsFalse()
        {
            var value = new string('a', 64) + 'l'; // append invalid character
            var result = Base32EConverter.IsEmailBase32(value);
            Assert.That(result, Is.False);
        }
    }
}
