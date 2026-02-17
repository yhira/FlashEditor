using FluentAssertions;
using Xunit;

namespace FlashEditor.Tests;

public class HistoryManagerTests
{
    // テスト対象のインスタンスを毎回新しく生成するヘルパー
    private HistoryManager CreateSut() => new HistoryManager();

    // ===== 基本動作テスト =====

    [Fact]
    public void 初期状態ではUndoもRedoもできない()
    {
        var sut = CreateSut();

        sut.CanUndo.Should().BeFalse();
        sut.CanRedo.Should().BeFalse();
    }

    [Fact]
    public void Pushした後にUndoできるようになる()
    {
        var sut = CreateSut();

        // 十分な差分（10文字以上）を持つテキストをPush
        sut.Push("初期テキスト");
        sut.Push("これは十分に長い変更テキストです。差分が10文字以上あります。");

        sut.CanUndo.Should().BeTrue();
    }

    [Fact]
    public void UndoするとPush前のスナップショットに戻る()
    {
        var sut = CreateSut();

        // スナップショット1をPush
        sut.Push("最初の状態");
        // スナップショット2をPush（十分な差分）
        sut.Push("これは大幅に変更された二番目の状態のテキストです");

        // Undo: 現在の未保存テキスト（=スナップショット2と同じ内容）を渡す
        // → スタックからスナップショット2（"これは大幅に..."）がPopされる
        var result = sut.Undo("これは大幅に変更された二番目の状態のテキストです");
        // Popされたのはスタックのtop → "これは大幅に変更された二番目の状態のテキストです"
        // もう1回Undoして初期状態を取得
        var result2 = sut.Undo(result);
        result2.Should().Be("最初の状態");
    }

    [Fact]
    public void UndoしたあとRedoで元に戻せる()
    {
        var sut = CreateSut();

        sut.Push("状態A");
        sut.Push("状態Bは長い変更を含むテキストです。十分な差分量があります。");

        // 現在テキスト="状態Bは..." → Undo → スタックtop "状態Bは..." がPop
        var afterUndo1 = sut.Undo("状態Bは長い変更を含むテキストです。十分な差分量があります。");
        // afterUndo1 == "状態Bは..." (topをポップ)
        // もう一度Undo
        var afterUndo2 = sut.Undo(afterUndo1);
        afterUndo2.Should().Be("状態A");

        // Redo → Redoスタックから復帰
        var afterRedo = sut.Redo(afterUndo2);
        afterRedo.Should().Be("状態Bは長い変更を含むテキストです。十分な差分量があります。");
    }

    // ===== エッジケース =====

    [Fact]
    public void 空のスタックでUndoしても元のテキストが返る()
    {
        var sut = CreateSut();

        var result = sut.Undo("変更なし");

        result.Should().Be("変更なし");
    }

    [Fact]
    public void 空のスタックでRedoしても元のテキストが返る()
    {
        var sut = CreateSut();

        var result = sut.Redo("変更なし");

        result.Should().Be("変更なし");
    }

    [Fact]
    public void 同じテキストを連続Pushしても重複登録されない()
    {
        var sut = CreateSut();

        sut.Push("同じテキスト");
        sut.Push("同じテキスト");
        sut.Push("同じテキスト");

        // 1回しかPushされていないので、Undoでスタックが空になる
        sut.Undo("同じテキスト");
        sut.CanUndo.Should().BeFalse();
    }

    // ===== 最小変化しきい値テスト =====

    [Fact]
    public void 長さの差が10文字未満かつ行数変化が少ない場合はPushがスキップされる()
    {
        var sut = CreateSut();

        // 最初のテキストをPush（20文字）
        sut.Push("ABCDEFGHIJ0123456789");

        // 5文字だけ長さが変化（10文字未満）、行数変化なし → スキップされるはず
        sut.Push("ABCDEFGHIJ01234567890ABCD");

        // スキップされたのでUndoスタックには1つだけ
        sut.Undo("ABCDEFGHIJ01234567890ABCD");
        sut.CanUndo.Should().BeFalse();
    }

    [Fact]
    public void 長さの差がゼロで内容だけ違う場合はPushされる()
    {
        var sut = CreateSut();

        // 同じ長さの異なるテキスト: diff=0 なので diff>0 が false → しきい値チェックに入らない → Pushされる
        sut.Push("ABCDEFGHIJ");
        sut.Push("1234567890");

        // 2つPushされているのでUndoで戻れる
        var result = sut.Undo("1234567890");
        result.Should().Be("1234567890");
        // もう一度Undo
        var result2 = sut.Undo(result);
        result2.Should().Be("ABCDEFGHIJ");
    }

    [Fact]
    public void 行数が2行以上変わると小さな長さ差分でもPushされる()
    {
        var sut = CreateSut();

        sut.Push("行1のテキストです。最低限の長さ");
        // 長さ差分は少ないが、行数が2行以上増える
        sut.Push("行1の変更テキスト\n行2です\n行3です");

        // 行数変化があるので記録されたはず → Undo2回で最初のテキストに戻れる
        var undone1 = sut.Undo("行1の変更テキスト\n行2です\n行3です");
        var undone2 = sut.Undo(undone1);
        undone2.Should().Be("行1のテキストです。最低限の長さ");
    }

    [Fact]
    public void 大きな差分があればPushされる()
    {
        var sut = CreateSut();

        sut.Push("短い");
        // 10文字以上の差分
        sut.Push("これは十分に長い変更のテキストです");

        // Undo2回で元に戻れる
        var undone1 = sut.Undo("これは十分に長い変更のテキストです");
        var undone2 = sut.Undo(undone1);
        undone2.Should().Be("短い");
    }

    // ===== Redoクリアテスト =====

    [Fact]
    public void 新しいPushでRedoスタックがクリアされる()
    {
        var sut = CreateSut();

        sut.Push("テキストA");
        sut.Push("テキストBは十分に長い差分を持つ変更です。しっかり記録されます。");

        // Undoを1回実行 → Redoスタックに入る
        sut.Undo("テキストBは十分に長い差分を持つ変更です。しっかり記録されます。");
        sut.CanRedo.Should().BeTrue();

        // 新しいテキストをPush → Redoスタックがクリアされる
        sut.Push("テキストCは完全に新しい方向の大幅な変更内容です。");
        sut.CanRedo.Should().BeFalse();
    }

    // ===== スタック上限テスト =====

    [Fact]
    public void スタックが1000件を超えると古いエントリが削除される()
    {
        var sut = CreateSut();

        // 1001件Push（各テキストの差分が10文字以上になるようにする）
        for (int i = 0; i <= 1000; i++)
        {
            sut.Push($"エントリ番号{i:D5} - このテキストは十分に長い差分を満たします");
        }

        // 1000件以下に収まっているはず
        int undoCount = 0;
        while (sut.CanUndo)
        {
            sut.Undo("dummy");
            undoCount++;
        }
        undoCount.Should().BeLessThanOrEqualTo(1000);
    }

    // ===== Save/Load テスト =====

    [Fact]
    public void SaveしたスタックをLoadで正しく復元できる()
    {
        // 一時ファイルを使用
        var tempFile = Path.GetTempFileName();
        try
        {
            var original = CreateSut();
            original.Push("セーブテスト1");
            original.Push("セーブテスト2は十分に長い差分を持つテキストです。20文字以上ある。");
            original.Push("セーブテスト3はさらに異なる長いテキストで、履歴に確実に記録されます。");

            // Undoを1回した状態でSave
            var undone = original.Undo("現在の編集中テキスト");
            // undone = "セーブテスト3は..." (スタックtopをPop)

            original.Save(tempFile);

            // 別のインスタンスでLoad
            var loaded = CreateSut();
            loaded.Load(tempFile);

            // Undo/Redo状態が復元されている
            loaded.CanUndo.Should().BeTrue();
            loaded.CanRedo.Should().BeTrue();

            // Redoで「現在の編集中テキスト」が復元される（Undo時にRedoスタックに入れた値）
            var redone = loaded.Redo("dummy");
            redone.Should().Be("現在の編集中テキスト");
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void 存在しないファイルをLoadしてもエラーにならない()
    {
        var sut = CreateSut();

        var act = () => sut.Load("nonexistent_file.dat");
        act.Should().NotThrow();
    }

    // ===== 複数回のUndo/Redoテスト =====

    [Fact]
    public void 複数回のUndoとRedoが正しく動作する()
    {
        var sut = CreateSut();

        // 大きな差分で3つの状態を積む（各テキスト間の長さ差が10文字以上）
        sut.Push("テキスト1");
        sut.Push("テキスト2 - 大幅に変更された二番目の状態です。十分な差分があります。");
        sut.Push("テキスト3 - さらに大きく変わった三番目の最終状態のテキストです。これは非常に長いテキストで確実にしきい値を超えます。");

        // Undo 3回（スタックから全てPop）
        var u1 = sut.Undo("テキスト3 - さらに大きく変わった三番目の最終状態のテキストです。これは非常に長いテキストで確実にしきい値を超えます。");
        var u2 = sut.Undo(u1);
        u2.Should().Be("テキスト2 - 大幅に変更された二番目の状態です。十分な差分があります。");

        var u3 = sut.Undo(u2);
        u3.Should().Be("テキスト1");

        // Redo 2回
        var r1 = sut.Redo(u3);
        r1.Should().Be("テキスト2 - 大幅に変更された二番目の状態です。十分な差分があります。");

        var r2 = sut.Redo(r1);
        r2.Should().Be("テキスト3 - さらに大きく変わった三番目の最終状態のテキストです。これは非常に長いテキストで確実にしきい値を超えます。");
    }
}
