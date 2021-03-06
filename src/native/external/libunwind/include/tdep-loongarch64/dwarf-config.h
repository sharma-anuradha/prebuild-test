/* libunwind - a platform-independent unwind library
   Copyright (C) 2008 CodeSourcery
   Copyright (C) 2012 Tommi Rantala <tt.rantala@gmail.com>
   Copyright (C) 2021 Loongson Technology Corporation Limited

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

#ifndef dwarf_config_h
#define dwarf_config_h

/* This is FIRST_PSEUDO_REGISTER in GCC, since DWARF_FRAME_REGISTERS is not
   explicitly defined.
   Number of hardware registers.  We have:
     - 32 integer registers
     - 32 floating point registers
     - 8 condition code registers
     - 2 fake registers:
        - ARG_POINTER_REGNUM
        - FRAME_POINTER_REGNUM  */
#define DWARF_NUM_PRESERVED_REGS	74

#define dwarf_to_unw_regnum(reg) (((reg) < 32) ? (reg) : 0)

/* Not big-endian.  */
#define dwarf_is_big_endian(addr_space)	0

/* Convert a pointer to a dwarf_cursor structure to a pointer to
   unw_cursor_t.  */
#define dwarf_to_cursor(c)      ((unw_cursor_t *) (c))

typedef struct dwarf_loc
  {
    unw_word_t val;
#ifndef UNW_LOCAL_ONLY
    unw_word_t type;            /* see DWARF_LOC_TYPE_* macros.  */
#endif
  }
dwarf_loc_t;

#endif /* dwarf_config_h */
