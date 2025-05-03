package com.d3t.rider.plugins.usages.options

import com.d3t.rider.plugins.usages.OptionPagesBundle
import com.jetbrains.rider.settings.simple.SimpleOptionsPage

class FunctionMacroUsagesOptionsPage : SimpleOptionsPage(
    name = OptionPagesBundle.message("configurable.name.optionpages.options.title"),
    pageId = "FunctionMacroUsagesOptions" // Must be in sync with SamplePage.PID
) {
    override fun getId(): String {
        return "FunctionMacroUsagesOptions"
    }
}