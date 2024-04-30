# PdfBoss Manual
version 1 (30 Apr 2024)

Use PdfBoss to optimise, merge and split PDF files.

This is a simple exercise that exposes a subset of features available in qpdf and the qpdfNet package. For now, the interface is practical yet meager, especially when compared to the amount of options available in the command line tool.

## Usage

### 1. Optimisation and size reduction

To reduce the size of individual PDF files, load one or more PDF files and click "Optimise". Optimised files are always generated and are named with a suffix of your choice. This is to prevent inadvertently writing to the original files.

### 2. Merging files

To merge two or more PDF files, navigate to "Settings", and select the process mode "Merge Files". In the settings tab, you may also define an output directory for processed files. If an output directory is not selected, by default, the merged file should end up in your "MyDocuments\PdfBoss" folder. Back in the first tab, you can order the files by clicking the up and down arrow buttons, and exclude a single file by clicking the "X" button. When you perform "Optimise", the generated merged file is also reduced for you, you do not have to optimise it again.

### 3. Splitting pages

To split pages from a PDF, load the file and select it, right click it on the grid and use the "Split Pages" command. This action splits the pages into separate, individual PDF files. The output is generated according to the settings you've chosen.

## Optional ghostscript pass

You have the option to use ghostscript as an additional pre-optimiser pass. Navigate to "Settings", select "Use GhostScript" and pick the location where you installed the ghostscript executable "gswin64c.exe". This may generate smaller files, at the cost of reduced image quality.

## Settings (for tech enthusiasts)

There are many ways to configure the flags and switches available. The following choices are an attempt at reasonable sane defaults, for general usage, in this interface.

### High compression

This setting applies zlib compression level 9, compress streams, uses compress stream data, recompress flate, generate object streams and optimise images.

If using ghostscript, it does a pre-pass with pdf setting "/screen".

### Medium compression

Same as high, with zlib compression level 7 and added linearization. What linearize does is add an extra little bit of data, at the start of the file, to allow immediate rendering. Useful for web viewing.

If using ghostscript, it does a pre-pass with pdf setting "/ebook".

### Low compression

Does zlib compression level 3, optimise images and linearization.

If using ghostscript, it does a pre-pass with pdf setting "/print".
