namespace Legend2Toolbox.Application.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    public record DummyRequest : IRequest<string>;


    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var request = new DummyRequest();
        var validatorMock = Substitute.For<IValidator<DummyRequest>>();
        var validationFailure = new ValidationFailure("Property", "Error message");

        validatorMock.ValidateAsync(Arg.Any<ValidationContext<DummyRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { validationFailure }));

        var behavior = new ValidationBehavior<DummyRequest, string>([validatorMock]);

        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

        // Act
        // Assert
        var act = async () => await behavior.Handle(request, next, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>()
            .Where(e => e.Errors.Any(err => err.ErrorMessage == "Error message"));
    }


    [Fact]
    public async Task Handle_ShouldInvokeNext_WhenValidationPasses()
    {
        // Arrange
        var request = new DummyRequest();
        var validatorMock = Substitute.For<IValidator<DummyRequest>>();

        validatorMock.ValidateAsync(Arg.Any<ValidationContext<DummyRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<DummyRequest, string>(new[] { validatorMock });
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success Result");

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.Should().Be("Success Result");
    }



}
