using System;
using System.Threading;
using System.Windows.Forms;
using FluentAssertions;
using Xunit;

namespace FlashEditor.Tests;

public class SearchHelperTests
{
    // RichTextBoxなどのWinFormsコントロールはSTAスレッドで操作する必要があるため、
    // テストをSTAスレッド内で実行するヘルパーメソッド
    private void RunInSta(Action action)
    {
        Exception? threadEx = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                threadEx = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (threadEx != null)
        {
            throw threadEx; // 例外があればテストを失敗させる
        }
    }

    [Fact]
    public void CountAllMatches_ReturnsCorrectTotalAndCurrentIndex()
    {
        RunInSta(() =>
        {
            using var rtb = new RichTextBox();
            rtb.Text = "Apple banana Apple cherry apple";

            // "Apple" (大文字小文字を区別する) -> index 0, 13 がマッチ
            var result1 = SearchHelper.CountAllMatches(rtb, "Apple", 0, matchCase: true, wholeWord: false);
            result1.totalCount.Should().Be(2);
            result1.currentIndex.Should().Be(1); // 0は1番目

            var result2 = SearchHelper.CountAllMatches(rtb, "Apple", 13, matchCase: true, wholeWord: false);
            result2.totalCount.Should().Be(2);
            result2.currentIndex.Should().Be(2); // 13は2番目

            // "apple" (区別しない) -> 0, 13, 26 のすべてマッチ
            var result3 = SearchHelper.CountAllMatches(rtb, "apple", 26, matchCase: false, wholeWord: false);
            result3.totalCount.Should().Be(3);
            result3.currentIndex.Should().Be(3); // 26は3番目
        });
    }

    [Fact]
    public void CountAllMatches_WholeWord_WorksCorrectly()
    {
        RunInSta(() =>
        {
            using var rtb = new RichTextBox();
            // "app" は単独の単語として1回、"apple" の中に1回出現する
            rtb.Text = "I have an app and an apple.";

            // 区別しない場合
            var resultPartial = SearchHelper.CountAllMatches(rtb, "app", 10, matchCase: false, wholeWord: false);
            resultPartial.totalCount.Should().Be(2);
            resultPartial.currentIndex.Should().Be(1);

            // 単語単位の場合
            var resultWhole = SearchHelper.CountAllMatches(rtb, "app", 10, matchCase: false, wholeWord: true);
            resultWhole.totalCount.Should().Be(1);
            resultWhole.currentIndex.Should().Be(1);
        });
    }

    [Fact]
    public void CountAllMatches_WhenNotFound_ReturnsZero()
    {
        RunInSta(() =>
        {
            using var rtb = new RichTextBox();
            rtb.Text = "Hello world";

            var result = SearchHelper.CountAllMatches(rtb, "test", -1, matchCase: false, wholeWord: false);
            result.totalCount.Should().Be(0);
            result.currentIndex.Should().Be(0);
        });
    }
}
