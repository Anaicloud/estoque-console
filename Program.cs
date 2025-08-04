using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace EstoqueApp
{
    public class Produto
    {
        public required string Nome { get; init; }
        public required decimal Preco { get; init; }
        public int Quantidade { get; set; }
        public decimal ValorTotal => Preco * Quantidade;
    }

    class Program
    {
        const string ArquivoEstoque = "estoque.json";

        static void Main()
        {
            var listaProdutos = CarregarEstoque();

            while (true)
            {
                Console.WriteLine("\n=== MENU DE ESTOQUE ===");
                Console.WriteLine("1 - Listar produtos");
                Console.WriteLine("2 - Adicionar produto");
                Console.WriteLine("3 - Remover quantidade de produto");
                Console.WriteLine("4 - Ver valor total do estoque");
                Console.WriteLine("5 - Salvar e sair");
                Console.Write("Escolha uma opção: ");
                var opcao = Console.ReadLine()?.Trim();

                switch (opcao)
                {
                    case "1":
                        ListarProdutos(listaProdutos);
                        break;
                    case "2":
                        AdicionarProduto(listaProdutos);
                        break;
                    case "3":
                        RemoverQuantidadeProduto(listaProdutos);
                        break;
                    case "4":
                        MostrarValorTotal(listaProdutos);
                        break;
                    case "5":
                        SalvarEstoque(listaProdutos);
                        Console.WriteLine("Estoque salvo. Tchau! 👋");
                        return;
                    default:
                        Console.WriteLine("Opção inválida. Tente novamente.");
                        break;
                }
            }
        }

        static List<Produto> CarregarEstoque()
        {
            if (!File.Exists(ArquivoEstoque))
                return new List<Produto>
                {
                    new() { Nome = "Caderno", Preco = 12.5m, Quantidade = 10 },
                    new() { Nome = "Caneta", Preco = 2.3m, Quantidade = 50 },
                    new() { Nome = "Mochila", Preco = 89.9m, Quantidade = 5 }
                };

            try
            {
                var json = File.ReadAllText(ArquivoEstoque);
                var produtos = JsonSerializer.Deserialize<List<Produto>>(json);
                return produtos ?? new List<Produto>();
            }
            catch
            {
                Console.WriteLine("Erro ao carregar o arquivo de estoque. Iniciando com padrão.");
                return new List<Produto>();
            }
        }

        static void SalvarEstoque(List<Produto> produtos)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(produtos, options);
            File.WriteAllText(ArquivoEstoque, json);
        }

        static void ListarProdutos(List<Produto> produtos)
        {
            Console.WriteLine("\n--- Produtos em estoque ---");
            var disponiveis = produtos.Where(p => p.Quantidade > 0)
                                      .OrderByDescending(p => p.Preco)
                                      .ToList();

            if (!disponiveis.Any())
            {
                Console.WriteLine("Nenhum produto disponível no estoque.");
                return;
            }

            foreach (var p in disponiveis)
            {
                Console.WriteLine($"{p.Nome} - Preço: R${p.Preco:F2}, Quantidade: {p.Quantidade}, Total: R${p.ValorTotal:F2}");
            }
        }

        static void AdicionarProduto(List<Produto> produtos)
        {
            Console.Write("Digite o nome do produto: ");
            var nome = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(nome))
            {
                Console.WriteLine("Nome inválido.");
                return;
            }

            Console.Write("Digite o preço (ex: 12.50): ");
            if (!decimal.TryParse(Console.ReadLine()?.Replace(',', '.'), out var preco) || preco < 0)
            {
                Console.WriteLine("Preço inválido.");
                return;
            }

            Console.Write("Digite a quantidade: ");
            if (!int.TryParse(Console.ReadLine(), out var quantidade) || quantidade < 0)
            {
                Console.WriteLine("Quantidade inválida.");
                return;
            }

            var existente = produtos.FirstOrDefault(p => p.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase));
            if (existente != null)
            {
                existente.Quantidade += quantidade;
                Console.WriteLine($"Produto atualizado: {existente.Nome} agora tem {existente.Quantidade} unidades.");
            }
            else
            {
                produtos.Add(new Produto { Nome = nome, Preco = preco, Quantidade = quantidade });
                Console.WriteLine($"Produto adicionado: {nome} com {quantidade} unidades.");
            }
        }

        static void RemoverQuantidadeProduto(List<Produto> produtos)
        {
            Console.Write("Digite o nome do produto para remover: ");
            var nome = Console.ReadLine()?.Trim();
            var produto = produtos.FirstOrDefault(p => p.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase));
            if (produto == null)
            {
                Console.WriteLine("Produto não encontrado.");
                return;
            }

            Console.Write($"Quantidade a remover de {produto.Nome} (estoque atual: {produto.Quantidade}): ");
            if (!int.TryParse(Console.ReadLine(), out var remover) || remover <= 0)
            {
                Console.WriteLine("Quantidade inválida.");
                return;
            }

            if (remover >= produto.Quantidade)
            {
                produto.Quantidade = 0;
                Console.WriteLine($"{produto.Nome} agora está sem estoque.");
            }
            else
            {
                produto.Quantidade -= remover;
                Console.WriteLine($"{produto.Nome} agora tem {produto.Quantidade} unidades.");
            }
        }

        static void MostrarValorTotal(List<Produto> produtos)
        {
            var total = produtos.Where(p => p.Quantidade > 0).Sum(p => p.ValorTotal);
            Console.WriteLine($"\nValor total do estoque: R${total:F2}");
        }
    }
}
