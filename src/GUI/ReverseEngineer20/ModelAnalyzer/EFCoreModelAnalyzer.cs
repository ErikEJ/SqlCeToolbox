namespace ReverseEngineer20
{
    public class EFCoreModelAnalyzer
    {
        public string GenerateDebugView(dynamic context)
        {
            return CreateDebugView(context);
        }

        public string GenerateDgmlContent(dynamic context)
        {
            string debugView = CreateDebugView(context);

            return null;
        }

        private string CreateDebugView(dynamic context)
        {
            var model = context.Model;
            string view = model.DebugView.View;

            return view;
        }
    }
}
