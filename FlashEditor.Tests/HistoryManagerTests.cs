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

        // Undo: currentTextがスタックtopと同一 → スキップされて「最初の状態」に戻る
        var result = sut.Undo("これは大幅に変更された二番目の状態のテキストです");
        result.Should().Be("最初の状態");
    }

    [Fact]
    public void UndoしたあとRedoで元に戻せる()
    {
        var sut = CreateSut();

        sut.Push("状態A");
        sut.Push("状態Bは長い変更を含むテキストです。十分な差分量があります。");

        // currentText="状態Bは..." → スキップロジックで「状態A」に直接戻る
        var afterUndo = sut.Undo("状態Bは長い変更を含むテキストです。十分な差分量があります。");
        afterUndo.Should().Be("状態A");

        // Redo → Redoスタックから復帰
        var afterRedo = sut.Redo(afterUndo);
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
        // currentText == top("1234567890") → スキップされて直接 "ABCDEFGHIJ" に戻る
        var result = sut.Undo("1234567890");
        result.Should().Be("ABCDEFGHIJ");
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
    public void スタックが200件を超えると古いエントリが削除される()
    {
        var sut = CreateSut();

        // 201件Push（各テキストの差分が10文字以上になるようにする）
        for (int i = 0; i <= 200; i++)
        {
            sut.Push($"エントリ番号{i:D5} - このテキストは十分に長い差分を満たします");
        }

        // 200件以下に収まっているはず
        int undoCount = 0;
        while (sut.CanUndo)
        {
            sut.Undo("dummy");
            undoCount++;
        }
        undoCount.Should().BeLessThanOrEqualTo(200);
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

        // Undo: currentText == top → スキップして次へ
        var u1 = sut.Undo("テキスト3 - さらに大きく変わった三番目の最終状態のテキストです。これは非常に長いテキストで確実にしきい値を超えます。");
        u1.Should().Be("テキスト2 - 大幅に変更された二番目の状態です。十分な差分があります。");

        var u2 = sut.Undo(u1);
        u2.Should().Be("テキスト1");

        // Redo 2回
        var r1 = sut.Redo(u2);
        r1.Should().Be("テキスト2 - 大幅に変更された二番目の状態です。十分な差分があります。");

        var r2 = sut.Redo(r1);
        r2.Should().Be("テキスト3 - さらに大きく変わった三番目の最終状態のテキストです。これは非常に長いテキストで確実にしきい値を超えます。");
    }

    // ===== スキップロジックのテスト =====

    [Fact]
    public void Undo時にスタックトップとcurrentTextが同一ならスキップされる()
    {
        var sut = CreateSut();

        sut.Push("初期状態のテキスト");
        sut.Push("変更後のテキストです。十分な長さの差分があります。");

        // currentText がスタックトップと同じ → スキップして「初期状態のテキスト」に戻る
        var result = sut.Undo("変更後のテキストです。十分な長さの差分があります。");
        result.Should().Be("初期状態のテキスト");
    }

    [Fact]
    public void Undo時にスタックトップとcurrentTextが異なればスキップされない()
    {
        var sut = CreateSut();

        sut.Push("初期状態のテキスト");
        sut.Push("変更後のテキストです。十分な長さの差分があります。");

        // currentText がスタックトップと異なる → そのままPop
        var result = sut.Undo("エディタで更にユーザーが入力した別のテキスト");
        result.Should().Be("変更後のテキストです。十分な長さの差分があります。");
    }

    [Fact]
    public void Redo時にスタックトップとcurrentTextが同一ならスキップされる()
    {
        var sut = CreateSut();

        sut.Push("状態1のテキスト");
        sut.Push("状態2は十分に長い変更テキストです。差分がしきい値を超えます。");

        // Undo: トップスキップで「状態1のテキスト」に戻る
        var afterUndo = sut.Undo("状態2は十分に長い変更テキストです。差分がしきい値を超えます。");
        afterUndo.Should().Be("状態1のテキスト");

        // Redo で「状態2」を復元。Redoスタックのトップは Undo 時に積んだ currentText と同じなのでスキップ
        // ただし Redo スタックには「状態2は...」が積まれているはず
        var afterRedo = sut.Redo(afterUndo);
        afterRedo.Should().Be("状態2は十分に長い変更テキストです。差分がしきい値を超えます。");
    }
}
