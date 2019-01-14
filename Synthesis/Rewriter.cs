namespace Synthesis
{
    using System;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class Rewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var varDec = node.ChildNodes().OfType<VariableDeclarationSyntax>().FirstOrDefault();

            var predefType = varDec.ChildNodes().OfType<PredefinedTypeSyntax>().FirstOrDefault();

            var expr = varDec.ChildNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault()
                .ChildNodes().OfType<EqualsValueClauseSyntax>().FirstOrDefault()
                .ChildNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();

            var decl = varDec.ChildNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();

            var exprName = expr.ChildNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();

            var newRight = SyntaxFactory.TriviaList();

            if (predefType.ToString().Equals("int") && exprName.ToString().Equals("Range"))
            {
                var argList = expr.ChildNodes().OfType<ArgumentListSyntax>().FirstOrDefault();

                var argListCount = argList.ChildNodes().OfType<ArgumentSyntax>().Count();
                if (argListCount == 2)
                {
                    var arg1 = argList.ChildNodes().OfType<ArgumentSyntax>().FirstOrDefault();
                    var arg2 = argList.ChildNodes().OfType<ArgumentSyntax>().Last();

                    var right = new SyntaxNodeOrToken[(int.Parse(arg2.ToString()) - int.Parse(arg1.ToString()) + 1) * 2 - 1];

                    int count = 0;
                    for (int i = int.Parse(arg1.ToString()); i <= int.Parse(arg2.ToString()); i++)
                    {
                        var val = SyntaxFactory.LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(i));

                        right[count] = val;
                        count++;
                        if (i < int.Parse(arg2.ToString()))
                        {
                            right[count] = SyntaxFactory.Token(SyntaxKind.CommaToken);
                            count++;
                        }
                    }

                    for (int i = 0; i < count; i++)
                    {
                        Console.Write(right[i]);
                    }
                    Console.WriteLine();

                    

                    var newFieldDecl = SyntaxFactory.FieldDeclaration(
        SyntaxFactory.VariableDeclaration(
            SyntaxFactory.ArrayType(
                SyntaxFactory.PredefinedType(
                    SyntaxFactory.Token(SyntaxKind.IntKeyword)))
            .WithRankSpecifiers(
                SyntaxFactory.SingletonList<ArrayRankSpecifierSyntax>(
                    SyntaxFactory.ArrayRankSpecifier(
                        SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                            SyntaxFactory.OmittedArraySizeExpression())))))
        .WithVariables(
            SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                SyntaxFactory.VariableDeclarator(
                    SyntaxFactory.Identifier(decl.Identifier.ToString()))
                .WithInitializer(
                    SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.ImplicitArrayCreationExpression(
                            SyntaxFactory.InitializerExpression(
                                SyntaxKind.ArrayInitializerExpression,
                                SyntaxFactory.SeparatedList<ExpressionSyntax>(
                                    right))))))))
    .NormalizeWhitespace();

                    Console.WriteLine(newFieldDecl);

                    node = node.ReplaceNode(node, newFieldDecl);
                }
            }
            return base.VisitFieldDeclaration(node).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
        }
    }
}
