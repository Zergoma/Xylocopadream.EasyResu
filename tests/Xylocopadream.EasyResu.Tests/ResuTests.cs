using FluentAssertions;

namespace Xylocopadream.EasyResu.Tests;

public class ResuTests
{
    private enum Outcome
    {
        Success,
        NetworkError
    }

    [Fact]
    public void Error_without_value_on_enum_has_no_value()
    {
        // default(Outcome) is Success: it must never leak as a value
        Resu<Outcome> resu = Resu<Outcome>.Error("boom");

        resu.IsError.Should().BeTrue();
        resu.HasValue.Should().BeFalse();
        resu.TryGetValue(out _).Should().BeFalse();
    }

    [Fact]
    public void ErrorWith_carries_the_detailed_code()
    {
        Resu<Outcome> resu = Resu<Outcome>.ErrorWith(Outcome.NetworkError, "offline");

        resu.IsError.Should().BeTrue();
        resu.TryGetValue(out Outcome code).Should().BeTrue();
        code.Should().Be(Outcome.NetworkError);
        resu.GetMessage.Should().Be("offline");
    }

    [Fact]
    public void Ok_carries_the_payload()
    {
        Resu<string> resu = Resu<string>.Ok("token");

        resu.IsSuccess.Should().BeTrue();
        resu.TryGetValue(out string? value).Should().BeTrue();
        value.Should().Be("token");
    }

    [Fact]
    public void Error_on_string_payload_treats_argument_as_message()
    {
        Resu<string> resu = Resu<string>.Error("message");

        resu.HasValue.Should().BeFalse();
        resu.Message.Should().Be("message");
    }

    [Fact]
    public void Ok_with_null_value_is_rejected()
    {
        Action act = () => Resu<string>.Ok(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WarningWith_keeps_value_and_status()
    {
        Resu<Outcome> resu = Resu<Outcome>.WarningWith(Outcome.Success, "partial");

        resu.IsWarning.Should().BeTrue();
        resu.HasValue.Should().BeTrue();
    }

    [Fact]
    public void GetMessage_falls_back_to_exception_message()
    {
        Resu resu = Resu.Error(ex: new InvalidOperationException("from exception"));

        resu.GetMessage.Should().Be("from exception");
    }

    [Fact]
    public void Both_types_can_be_handled_through_IResu()
    {
        IResu[] outcomes = [Resu.Ok(), Resu<int>.Error("x")];

        outcomes.Select(o => o.Status).Should().Equal(ResultStatus.Success, ResultStatus.Error);
    }
}
