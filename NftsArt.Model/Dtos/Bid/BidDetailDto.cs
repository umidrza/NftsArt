﻿using NftsArt.Model.Dtos.User;

namespace NftsArt.Model.Dtos.Bid;

public record class BidDetailDto(
        int Id,
        decimal Amount,
        DateTime StartTime,
        DateTime EndTime,
        int Quantity,
        UserSummaryDto Bidder
    );
