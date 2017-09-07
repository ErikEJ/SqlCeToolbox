namespace ReverseEngineer20
{
    public class EFCoreModelAnalyzer
    {
        public string GenerateDebugView(dynamic context)
        {
            var model = context.Model;

            string view = model.DebugView.View;

            return view;
        }
    }
}
