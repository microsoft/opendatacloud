// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Msr.Odr.Model;
using Xunit;

namespace Msr.Odr.Services.Test
{
    public class UtilsTests
    {
        public const bool Valid = true;
        public const bool Invalid = false;

        [Theory]
        [InlineData("david.jones@proseware.com", Valid)]
        [InlineData("d.j@server1.proseware.com", Valid)]
        [InlineData("jones@ms1.proseware.com", Valid)]
        [InlineData("j.@server1.proseware.com", Invalid)]
        [InlineData("j@proseware.com9", Valid)]
        [InlineData("js#internal@proseware.com", Valid)]
        [InlineData("j_9@[129.126.118.1]", Valid)]
        [InlineData("j..s@proseware.com", Invalid)]
        [InlineData("js*@proseware.com", Invalid)]
        [InlineData("js@proseware..com", Invalid)]
        [InlineData("js@proseware.com9", Valid)]
        [InlineData("j.s@server1.proseware.com", Valid)]
        [InlineData("\"j\\\"s\\\"\"@proseware.com", Valid)]
        [InlineData("js@contoso.中国", Valid)]
        public void ShouldValidateEmailsAddresses(string email, bool shouldBeValid)
        {
            Utils.IsValidEmail(email).Should().Be(shouldBeValid);
        }
    }
}
