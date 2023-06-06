using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;

using WebApplication1.Logger;

[assembly: AspectOrder(typeof(LoggingAttribute))]
namespace WebApplication1.Logger;

public class LoggingAttribute : OverrideMethodAspect
{
    public override dynamic OverrideMethod()
    {
        try
        {
            var message = BuildInterpolatedString();
            var output = message.ToValue() as string;

            if (!string.IsNullOrWhiteSpace(output))
                Console.WriteLine(output);

            var result = meta.Proceed();
            return result;
        }
        catch (Exception e)
        {
            var failureMessage = new InterpolatedStringBuilder();
            failureMessage.AddText(meta.Target.Method.Name);
            failureMessage.AddText(" failed: ");
            failureMessage.AddExpression(e.Message);

            var output = failureMessage.ToValue() as string;
            if (!string.IsNullOrWhiteSpace(output))
                Console.WriteLine(output);

            throw;
        }
    }

    protected InterpolatedStringBuilder BuildInterpolatedString()
    {
        var i = meta.CompileTime(0);

        var stringBuilder = new InterpolatedStringBuilder();
        stringBuilder.AddText(string.Format("{0}.{1}", meta.Target.Type.Name, meta.Target.Method.Name));
        stringBuilder.AddText("(");

        foreach (var prop in meta.Target.Parameters)
        {
            var comma = i > 0 ? ", " : "";

            if (i > 0)
                stringBuilder.AddText(", ");

            var builder = new ExpressionBuilder();
            builder.AppendVerbatim("System.Text.Json.JsonSerializer.Serialize(");
            builder.AppendExpression(prop.CastTo<object>());
            builder.AppendVerbatim(")");

            stringBuilder.AddText(string.Format("{0} : ", prop.Name));
            stringBuilder.AddExpression(builder.ToExpression());

            i++;
        }

        stringBuilder.AddText(")");

        return stringBuilder;
    }
}
