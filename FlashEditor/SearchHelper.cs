using System.Windows.Forms;

namespace FlashEditor;

public static class SearchHelper
{
    /// <summary>
    /// テキスト全体を走査して総マッチ数と、現在位置が何番目かを返します。
    /// </summary>
    public static (int totalCount, int currentIndex) CountAllMatches(RichTextBox rtb, string searchText, int currentPos, bool matchCase, bool wholeWord)
    {
        RichTextBoxFinds options = RichTextBoxFinds.None;
        if (matchCase) options |= RichTextBoxFinds.MatchCase;
        if (wholeWord) options |= RichTextBoxFinds.WholeWord;

        int total = 0;
        int currentIndex = 0;
        int pos = 0;

        // 元の選択状態を記憶
        int originalStart = rtb.SelectionStart;
        int originalLength = rtb.SelectionLength;

        // 先頭から末尾まで繰り返し検索してカウント
        while (pos < rtb.TextLength)
        {
            int found = rtb.Find(searchText, pos, rtb.TextLength, options);
            if (found == -1) break;
            total++;
            // 現在選択中の位置と一致したら何番目かを記録
            if (found == currentPos) currentIndex = total;
            pos = found + searchText.Length;
        }

        // 選択位置を元に戻す（Find呼び出しで選択がずれるため）
        rtb.Select(originalStart, originalLength);

        return (total, currentIndex);
    }
}
