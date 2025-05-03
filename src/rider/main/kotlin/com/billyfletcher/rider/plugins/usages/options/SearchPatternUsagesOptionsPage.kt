package com.billyfletcher.rider.plugins.usages.options

import com.billyfletcher.rider.plugins.usages.OptionPagesBundle
import com.jetbrains.rider.settings.simple.SimpleOptionsPage

class SearchPatternUsagesOptionsPage : SimpleOptionsPage(
    name = OptionPagesBundle.message("configurable.name.optionpages.options.title"),
    pageId = "SearchPatternUsagesOptions" // Must be in sync with SamplePage.PID
) {
    override fun getId(): String {
        return "SearchPatternUsagesOptions"
    }
}