using System;

namespace hagaki
{
    // メインテーブル
    public enum MainTableColumn
    {
        KanriNo,
        UkeDate,
        ZipCd,
        Add1,
        Add2,
        Add3,
        Add4,
        NameSei,
        NameMei,
        TelNo,
        Ank1,
        Ank2,
        Ank3,
        JyotaiKb,
        NgOutKb,
        HisoOutKb
    }

    // エラーテーブル
    public enum ErrorTableColumn
    {
        KanriNo,
        ErrCd
    }

    // 取り込み不可エラー
    public enum ErrorCd
    {
        NoError,
        LayoutError,
        IncorrectControlNumber,
        ImportedControlNumber,
        DuplicateControlNumber,
        IncorrectReceptionDate,
        DBSizeError
    }

    // 状態区分
    public enum JyotaiKb
    {
        Ok,
        Ng,
        Hold,
        Cancel
    }

    // NG票出力区分
    public enum NgOutKb
    {
        Un,
        Done
    }

    // 配送データ出力区分
    public enum HisoOutKb
    {
        Un,
        Done
    }
}
