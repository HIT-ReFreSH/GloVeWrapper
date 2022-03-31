using System.Threading.Tasks;
using HitRefresh.GloVeWrapper;
using PlasticMetal.MobileSuit;
using PlasticMetal.MobileSuit.Core;

namespace TestConsole;

[SuitInfo("GloVe Tools")]
public class Program
{
    public Program(IIOHub iO)
    {
        IO = iO;
    }

    private IIOHub IO { get; }

    private static async Task Main(string[] args)
    {
        //await GloVe.CreateBinaryAsync("/Projects/MCS/_suspend/glove.840B.300d/glove.840B.300d.txt");
        await Suit.CreateBuilder(args)
            .UsePowerLine()
            .MapClient<Program>()
            .Build().RunAsync();
    }

    [SuitInfo("Convert <txt-dict>")]
    [SuitAlias("c")]
    public async Task Convert(string txt)
    {
        await GloVe.CreateBinaryAsync(txt);
    }

    [SuitInfo("Distance <binary-dict> <word1> <word2>")]
    [SuitAlias("d")]
    public double Distance(string binPrefix, string word1, string word2)
    {
        using var bin = GloVe.AccessBinary(binPrefix);
        return GloVe.CosDistanceBetween(
            bin[word1], bin[word2]
        );
    }

    [SuitInfo("DistanceCached <binary-dict> <word1> <word2>")]
    [SuitAlias("dc")]
    public double DistanceCached(string binPrefix, string word1, string word2)
    {
        using var bin = GloVe.AccessBinary(binPrefix).AsCached();
        return GloVe.CosDistanceBetween(
            bin[word1], bin[word2]
        );
    }
}