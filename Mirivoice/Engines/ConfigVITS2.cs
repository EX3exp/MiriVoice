using Microsoft.Extensions.Primitives;
using Serilog;
using System;
using System.Text;
using VYaml.Annotations;
using System.Collections.Generic;
using System.Linq;

namespace Mirivoice.Engines
{
    [YamlObject]
    public partial class VITS2Model
    {
        public bool use_mel_posterior_encoder { get; set; } = true;
        public bool use_transformer_flows { get; set; } = true;
        public string transformer_flow_type { get; set; } = "mono_layer_inter_residual";
        public bool use_spk_conditioned_encoder { get; set; } = true;
        public bool use_noise_scaled_mas { get; set; } = true;
        public bool use_duration_discriminator { get; set; } = true;
        public string duration_discriminator_type { get; set; } = "dur_disc_2";
        public int inter_channels { get; set; } = 192;
        public int hidden_channels { get; set; } = 192;
        public int filter_channels { get; set; } = 768;
        public int n_heads { get; set; } = 2;
        public int n_layers { get; set; } = 10;
        public int kernel_size { get; set; } = 5;
        public double p_dropout { get; set; } = 0.1;
        public int resblock { get; set; } = 1;
        public int[] resblock_kernel_sizes { get; set; } = new int[] { 3, 7, 11 };
        public int[][] resblock_dilation_sizes { get; set; } = new int[][]
        {
                new int[] { 1, 3, 5 },
                new int[] { 1, 3, 5 },
                new int[] { 1, 3, 5 }
        };
        public int[] upsample_rates { get; set; } = new int[] { 8, 8, 2, 2 };
        public int upsample_initial_channel { get; set; } = 512;
        public int[] upsample_kernel_sizes { get; set; } = new int[] { 16, 16, 4, 4 };
        public int n_layers_q { get; set; } = 3;
        public bool use_spectral_norm { get; set; } = false;
        public bool use_sdp { get; set; } = false;
        public int gin_channels { get; set; } = 256;

        public VITS2Model()
        {
        }
    }

    [YamlObject]
    public partial class VITS2Data
    {
        public bool use_mel_posterior_encoder { get; set; } = true;
        public double max_wav_value { get; set; } = 32768.0;
        public int sampling_rate { get; set; } = 22050;
        public int filter_length { get; set; } = 1024;
        public int hop_length { get; set; } = 256;
        public int win_length { get; set; } = 1024;
        public int n_mel_channels { get; set; } = 80;
        public double mel_fmin { get; set; } = 0.0;
        public double? mel_fmax { get; set; } = null;

        public bool add_blank { get; set; } = false;
        public int n_speakers { get; set; } = 4;
        public bool cleaned_text { get; set; } = true;

        public VITS2Data()
        {
        }
    }

    [YamlObject]
    public partial class ConfigVITS2
    {
        public string ModelPath { get; set; }

        public string _pad {
            get
            {
                return "<pad>";
            }
            private set { }
        }

        public string _blank
        {
            get
            {
                return "<blank>";
            }
            private set { }
        }

        public string _punctuation {
            get
            {
                return ";:,.!?¡¿—…\"«»“” ".ToCharArray().Select(c => c.ToString()).Aggregate((a, b) => a + "\t" + b);
            }
            private set
            {
            }
        }
        public string _letters_ipa
        {
            get
            {
                return "!!	(	((	(.)	(..)	(...)	)	))	*	/	1	2	3	A	B	C	C	D	E	F	F	G	H	I	J	J	K	L	L	M	N	O	P	Q	R	S	T	U	V	V	W	W	X	Y	Z	[	\\	]	a	b	c	d	e	f	g	h	i	j	k	l	m	n	o	p	q	r	s	t	u	v	w	x	y	z	{	|	}	æ	ç	ð	ø	č	ħ	ı	ŋ	Œ	œ	š	ž	ƈ	ƙ	ƛ	ƞ	ƥ	ƫ	ƭ	ƻ	ǀ	ǁ	ǂ	ǃ	ǰ	ȡ	ȵ	ȶ	ɐ	ɑ	ɒ	ɓ	ɔ	ɕ	ɖ	ɗ	ɘ	ə	ɚ	ɛ	ɜ	ɞ	ɟ	ɠ	ɡ	ɢ	ɣ	ɤ	ɥ	ɦ	ɧ	ɨ	ɩ	ɪ	ɫ	ɬ	ɭ	ɮ	ɯ	ɰ	ɱ	ɲ	ɳ	ɴ	ɵ	ɶ	ɷ	ɸ	ɹ	ɺ	ɻ	ɼ	ɽ	ɾ	ʀ	ʁ	ʂ	ʃ	ʄ	ʆ	ʇ	ʈ	ʉ	ʊ	ʋ	ʌ	ʍ	ʎ	ʏ	ʐ	ʑ	ʒ	ʓ	ʔ	ʕ	ʖ	ʗ	ʘ	ʙ	ʚ	ʛ	ʜ	ʝ	ʞ	ʟ	ʠ	ʡ	ʢ	ʣ	ʤ	ʥ	ʦ	ʧ	ʨ	ʩ	ʪ	ʫ	ʭ	ʰ	ʰ	ʲ	ʶ	ʷ	ʸ	ʻ	ʼ	ˆ	ˇ	ˈ	ˌ	ː	ˑ	˖	˗	˞	ˠ	ˡ	ˢ	ˣ	ˤ	˥	˥˩	˦	˦˥	˧	˧˦˧	˨	˩	˩˥	˩˨	ˬ	ˬ	˭	˱	˲	˷	˹	̀	́	̂	̃	̄	̆	̇	̈	̋	̌	̏	̑	̖;ˎ	̗;ˏ	̘	̙	̚	̜	̝;˔	̞;˕	̟	̠	̡	̢	̣	̤	̥;̊	̥₎	̩	̪	̫	̬	̬₎	̯	̰	̴	̹	̺	̻	̼	̽	͆	̪͆	͇	͈	͉	͊	͋	͌	͍	͎	͡	͢	Θ	β	θ	λ	χ	ᵊ	ᶑ	ᶹ	ᶿ	᷄	᷅	᷈	‖	‿	ⁿ	₍̥	₍̥₎	₍̬	₍̬₎	↑	↑	↓	↓	↗	↘	❍	ⱱ	ꟸ	ꟹ	𝆏	𝆏𝆏	𝆑	𝆑𝆑	𝑎𝑙𝑙𝑒𝑔𝑟𝑜	𝑙𝑒𝑛𝑡𝑜";

            }
            private set
            {
            }
        }
           
        // mirivoice uses only ipa symbols


        public VITS2Model model { get; set; } = new VITS2Model();
        public VITS2Data data { get; set; } = new VITS2Data();

        [YamlIgnore]
        public List<string> symbols
        {
            get {
                // Export all symbols
                StringBuilder sb = new StringBuilder();
                sb.Append(_blank);
                sb.Append("\t");
                sb.Append(_pad);
                
                
                sb.Append("\t");
                sb.Append(_letters_ipa);
                sb.Append("\t");
                sb.Append(_punctuation);
                sb.Append("\t");
                sb.Append("<bos>\t<eos>\t<space>\t<blank>");
                return sb.ToString().Split("\t").ToList();
            }
            private set
            {
                symbols = value;
            }
        }



    }
    
}
