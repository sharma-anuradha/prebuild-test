/* libunwind - a platform-independent unwind library
   Copyright (C) 2008 CodeSourcery

This file is part of libunwind.

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.  */

#include "unwind_i.h"

HIDDEN int
tdep_access_reg (struct cursor *c, unw_regnum_t reg, unw_word_t *valp,
                 int write)
{
  dwarf_loc_t loc = DWARF_NULL_LOC;
  
  switch (reg)
    {
    case UNW_ARM_R15:
      if (write)
        c->dwarf.ip = *valp;            /* update the IP cache */
    case UNW_ARM_R0:
    case UNW_ARM_R1:
    case UNW_ARM_R2:
    case UNW_ARM_R3:
    case UNW_ARM_R4:
    case UNW_ARM_R5:
    case UNW_ARM_R6:
    case UNW_ARM_R7:
    case UNW_ARM_R8:
    case UNW_ARM_R9:
    case UNW_ARM_R10:
    case UNW_ARM_R11:
    case UNW_ARM_R12:
    case UNW_ARM_R14:
      loc = c->dwarf.loc[reg - UNW_ARM_R0];
      break;

    case UNW_ARM_R13:
    case UNW_ARM_CFA:
      if (write)
        return -UNW_EREADONLYREG;
      *valp = c->dwarf.cfa;
      return 0;

    /* FIXME: Initialise coprocessor & shadow registers?  */

    default:
      Debug (1, "bad register number %u\n", reg);
      return -UNW_EBADREG;
    }

  if (write)
    return dwarf_put (&c->dwarf, loc, *valp);
  else
    return dwarf_get (&c->dwarf, loc, valp);
}

/* FIXME for ARM.  */

HIDDEN int
tdep_access_fpreg (struct cursor *c, unw_regnum_t reg, unw_fpreg_t *valp,
                   int write)
{
  dwarf_loc_t loc = DWARF_NULL_LOC;
  switch (reg)
  {
    case UNW_ARM_D0:
    case UNW_ARM_D1:
    case UNW_ARM_D2:
    case UNW_ARM_D3:
    case UNW_ARM_D4:
    case UNW_ARM_D5:
    case UNW_ARM_D6:
    case UNW_ARM_D7:
    case UNW_ARM_D8:
    case UNW_ARM_D9:
    case UNW_ARM_D10:
    case UNW_ARM_D11:
    case UNW_ARM_D12:
    case UNW_ARM_D13:
    case UNW_ARM_D14:
    case UNW_ARM_D15:
      loc = c->dwarf.loc[UNW_ARM_S0 + (reg - UNW_ARM_D0)];
      break;

    default:
      Debug (1, "bad register number %u\n", reg);
      return -UNW_EBADREG;
  }

  if (write)
    return dwarf_putfp (&c->dwarf, loc, *valp);
  else
    return dwarf_getfp (&c->dwarf, loc, valp);
}
